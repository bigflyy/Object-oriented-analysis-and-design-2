"""Remove background from character image using rembg (silueta model)."""
import sys
from rembg import remove, new_session
from PIL import Image

session = new_session('silueta')

input_path = sys.argv[1]
output_path = sys.argv[2] if len(sys.argv) > 2 else input_path

img = Image.open(input_path)
result = remove(img, session=session)
result.save(output_path)
print("OK")
