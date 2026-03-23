"""
Test LLM storytelling quality across different temperatures and prompt styles.
Runs a full 10-turn story and evaluates diversity, context retention, and quality.
"""
import os, json, time, sys
os.environ.pop('HTTP_PROXY', None)
os.environ.pop('HTTPS_PROXY', None)
os.environ.pop('http_proxy', None)
os.environ.pop('https_proxy', None)
os.environ['NO_PROXY'] = '*'

import requests

MODEL = "qwen3.5:4b"
API = "http://127.0.0.1:11434/api/chat"

def llm_call(messages, temperature=1.0, num_ctx=8192):
    body = {
        "model": MODEL,
        "messages": messages,
        "stream": False,
        "think": False,
        "options": {"temperature": temperature, "num_gpu": 99, "num_ctx": num_ctx}
    }
    r = requests.post(API, json=body, timeout=120, proxies={'http':None,'https':None})
    if r.status_code == 200:
        return r.json()["message"]["content"]
    return f"[ERROR {r.status_code}]"

SYSTEM_PROMPT_V1 = """You are a second-person visual novel narrator. The player is the main character, referred to as 'you'.
Always respond in English.
RULES:
- Respond with ONLY a raw JSON object, nothing else.
- No markdown, no explanation, no preamble.
- Required fields:
  "speaker": string (Either "Narrator" or the NPC's name. Only ONE speaker per turn.)
  "characterAnswer": string (5-8 sentences. Rich, vivid, immersive.
  If speaker is "Narrator": describe what you see, hear, feel, smell. Paint the scene. Build tension or wonder.
  If speaker is an NPC: write AS that character in FIRST PERSON. Give them personality, quirks, emotion.
  NEVER include JSON or code in this field.)
  "isLocationChange": boolean
  "isCharacterChange": boolean
  "isAlone": boolean
  "isSceneUpdate": boolean
  "isCharacterUpdate": boolean
  "nextLocation": string
  "nextCharacter": string
  "sceneDescription": string
  "characterUpdateDescription": string
- Be dynamic! Change locations every 2-3 turns. Introduce new characters.
- When the player explores alone, set isAlone=true. Not every scene needs an NPC.
- When something dramatic happens, set isSceneUpdate=true with a visual description.
- When a character's appearance changes (emotion, injury), set isCharacterUpdate=true.
Output ONLY the JSON object:"""

SYSTEM_PROMPT_V2 = """You are the game master of an interactive visual novel. The player is 'you' - the protagonist.
Always respond in English. Never break character.
RULES:
- Output ONLY a raw JSON object. No markdown, no text outside JSON.
- Fields:
  "speaker": "Narrator" or NPC name (one speaker per turn)
  "characterAnswer": 5-8 sentences of rich storytelling. Make every turn feel like a chapter.
  As Narrator: use all five senses. Describe the light, the sounds, the texture of the air.
  As NPC: speak in first person with a unique voice. Show emotion through action, not just words.
  Example Narrator: 'The corridor stretches before you, its walls slick with moisture that catches the faint blue glow of bioluminescent moss. Your footsteps echo in a rhythm that feels almost like a heartbeat. Something moves in the shadows ahead - too fast to be human, too deliberate to be wind.'
  Example NPC (gruff captain): 'Look, I did not drag you halfway across the sector to watch you gawk at the scenery. We have got maybe six hours before the station locks down. So you can stand there looking pretty, or you can help me crack this console. Your call.'
  "isLocationChange": bool, "isCharacterChange": bool, "isAlone": bool
  "isSceneUpdate": bool, "isCharacterUpdate": bool
  "nextLocation": "", "nextCharacter": ""
  "sceneDescription": "", "characterUpdateDescription": ""
- PACING: alternate between action, dialogue, and quiet exploration.
- VARIETY: never repeat the same scene structure twice in a row.
- AGENCY: react meaningfully to the player's choices. Consequences matter.
Output ONLY the JSON:"""

def run_story(system_prompt, temperature, story_idea, player_actions, label):
    print(f"\n{'='*60}")
    print(f"TEST: {label} (temp={temperature})")
    print(f"{'='*60}")

    # Generate plot
    plot_msg = [
        {"role": "system", "content": "Create a 2-3 sentence plot for a second-person visual novel. Use 'you'. Respond with just the plot."},
        {"role": "user", "content": f"Idea: {story_idea}"}
    ]
    plot = llm_call(plot_msg, temperature)
    print(f"\nPLOT: {plot[:200]}")

    # Build memory
    memory = f"== PLOT ==\n{plot}\n\n"
    memory += "== CURRENT STATE ==\nYou are at: Starting Location\n\n"
    memory += "== RECENT HISTORY ==\n"

    turns = []
    for i, action in enumerate(player_actions):
        prompt = memory + f"\nPlayer action: {action}"
        messages = [
            {"role": "system", "content": system_prompt},
            {"role": "user", "content": prompt}
        ]

        t0 = time.time()
        raw = llm_call(messages, temperature)
        elapsed = time.time() - t0

        # Parse
        try:
            start = raw.find('{')
            end = raw.rfind('}')
            if start >= 0 and end > start:
                j = json.loads(raw[start:end+1])
                speaker = j.get("speaker", "Narrator")
                answer = j.get("characterAnswer", "")
                loc_change = j.get("isLocationChange", False)
                char_change = j.get("isCharacterChange", False)
                alone = j.get("isAlone", False)
                scene_update = j.get("isSceneUpdate", False)
                char_update = j.get("isCharacterUpdate", False)
                next_loc = j.get("nextLocation", "")
                next_char = j.get("nextCharacter", "")

                turns.append({
                    "action": action, "speaker": speaker, "answer": answer,
                    "loc_change": loc_change, "char_change": char_change,
                    "alone": alone, "scene_update": scene_update, "char_update": char_update,
                    "time": elapsed, "valid": True
                })

                # Update memory
                memory += f"- Player: {action}\n"
                memory += f"- [{speaker}]: {answer[:100]}...\n"
                if loc_change and next_loc:
                    memory = memory.replace("You are at: " + memory.split("You are at: ")[1].split("\n")[0],
                                           f"You are at: {next_loc}")

                print(f"\nTurn {i+1} ({elapsed:.1f}s) [{speaker}]:")
                print(f"  {answer[:150]}...")
                flags = []
                if loc_change: flags.append(f"LOC->{next_loc}")
                if char_change: flags.append(f"CHAR->{next_char}")
                if alone: flags.append("ALONE")
                if scene_update: flags.append("SCENE_UPD")
                if char_update: flags.append("CHAR_UPD")
                if flags: print(f"  Flags: {', '.join(flags)}")
            else:
                turns.append({"action": action, "answer": raw[:100], "valid": False, "time": elapsed})
                print(f"\nTurn {i+1}: PARSE FAILED - {raw[:100]}")
        except Exception as e:
            turns.append({"action": action, "answer": str(e), "valid": False, "time": elapsed})
            print(f"\nTurn {i+1}: ERROR - {e}")

    # Analyze
    valid = sum(1 for t in turns if t.get("valid"))
    loc_changes = sum(1 for t in turns if t.get("loc_change"))
    char_changes = sum(1 for t in turns if t.get("char_change"))
    alone_turns = sum(1 for t in turns if t.get("alone"))
    scene_updates = sum(1 for t in turns if t.get("scene_update"))
    char_updates = sum(1 for t in turns if t.get("char_update"))
    avg_len = sum(len(t.get("answer","")) for t in turns if t.get("valid")) / max(1, valid)
    avg_time = sum(t["time"] for t in turns) / len(turns)

    # Check diversity: unique first words
    first_words = set()
    for t in turns:
        if t.get("valid") and t.get("answer"):
            first_words.add(t["answer"].split()[0].lower() if t["answer"].split() else "")

    print(f"\n--- SUMMARY ---")
    print(f"Valid turns: {valid}/{len(turns)}")
    print(f"Avg answer length: {avg_len:.0f} chars")
    print(f"Avg time: {avg_time:.1f}s")
    print(f"Location changes: {loc_changes}")
    print(f"Character changes: {char_changes}")
    print(f"Alone turns: {alone_turns}")
    print(f"Scene updates: {scene_updates}")
    print(f"Character updates: {char_updates}")
    print(f"Unique first words: {len(first_words)}/{len(turns)} (diversity)")

    return {
        "label": label, "valid": valid, "total": len(turns),
        "avg_len": avg_len, "avg_time": avg_time,
        "loc_changes": loc_changes, "char_changes": char_changes,
        "alone": alone_turns, "scene_updates": scene_updates,
        "char_updates": char_updates, "diversity": len(first_words)
    }

# Player actions for a 10-turn story
actions = [
    "I look around carefully",
    "I walk towards the strange sound",
    "I try to talk to whoever is there",
    "I ask them about the danger they mentioned",
    "I decide to go investigate the ruins alone",
    "I search for clues in the rubble",
    "I hear footsteps behind me and turn around",
    "I run towards the exit",
    "I stop and face whatever is chasing me",
    "I try to reason with them"
]

results = []

# Test 1: Current prompt (v1) at temp 1.0
results.append(run_story(SYSTEM_PROMPT_V1, 1.0, "abandoned space station mystery", actions, "V1 temp=1.0"))

# Test 2: New prompt (v2) at temp 1.0
results.append(run_story(SYSTEM_PROMPT_V2, 1.0, "abandoned space station mystery", actions, "V2 temp=1.0"))

# Test 3: V2 at temp 0.8
results.append(run_story(SYSTEM_PROMPT_V2, 0.8, "abandoned space station mystery", actions, "V2 temp=0.8"))

# Test 4: V2 at temp 1.2
results.append(run_story(SYSTEM_PROMPT_V2, 1.2, "abandoned space station mystery", actions, "V2 temp=1.2"))

print(f"\n{'='*60}")
print("FINAL COMPARISON")
print(f"{'='*60}")
print(f"{'Label':<20} {'Valid':>5} {'AvgLen':>7} {'Time':>5} {'Loc':>4} {'Char':>5} {'Alone':>6} {'Scene':>6} {'ChUpd':>6} {'Div':>4}")
for r in results:
    print(f"{r['label']:<20} {r['valid']:>5} {r['avg_len']:>7.0f} {r['avg_time']:>5.1f} {r['loc_changes']:>4} {r['char_changes']:>5} {r['alone']:>6} {r['scene_updates']:>6} {r['char_updates']:>6} {r['diversity']:>4}")
