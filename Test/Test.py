import sys
import os 

sys.path.append(os.path.abspath(os.path.join(os.path.dirname(__file__), '..', '.github', 'scripts')))

import generate_archive

def test_rename_files_and_generate_json():
    src_directory = "C:\GrooperScripts\GrooperGit"  # Adjust this path as necessary
    generate_archive.rename_files_and_generate_json(src_directory, use_temp_dir=True)

# Run the test function
if __name__ == "__main__":
    test_rename_files_and_generate_json()