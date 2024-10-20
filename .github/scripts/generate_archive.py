import os
import json
import uuid
from datetime import datetime
import shutil
import tempfile
import argparse

def generate_guid():
    return str(uuid.uuid4())

def get_projectid():
    return '';
def current_datetime():
    return datetime.now().astimezone().isoformat()

def add_subdirectory(base_path, subdirectory):
    # Join the base path with the subdirectory
    new_path = os.path.join(base_path, subdirectory)

    try:
        # Create the new directory
        os.makedirs(new_path, exist_ok=True)
        print(f"Directory '{new_path}' created successfully.")
    except Exception as e:
        print(f"An error occurred: {e}")

def rename_files_and_generate_json(src_directory, use_temp_dir=False):
    if use_temp_dir:
        temp_directory = tempfile.mkdtemp()
    else:
        temp_directory = src_directory
        
    add_subdirectory(temp_directory, "Files")

    files_mapping = []
    for root, dirs, files in os.walk(src_directory):
        rootname = os.path.basename(root)
        for name in files:
            original_path = os.path.join(root, name,)
            new_name = f"{generate_guid()}_{rootname}•{name}.grp"
            new_path = os.path.join(temp_directory, "Files", new_name)
            if use_temp_dir:
                shutil.copy2(original_path, new_path)
            else:
                os.rename(original_path, new_path)
            files_mapping.append({"original": name, "new": new_name})

    archive_info = {
        "Author": "Steven T. Rotelli",
        "Publisher": "BIS",
        "ArchiveDate": current_datetime(),
        "Version": "23.1.0012",
        "Name": "GrooperThemes",
        "BaseNodeId": "f4d92776-554c-4633-ae37-e436e9fac79a",
        "SourceRepositoryId": "63193412-42d3-4922-9c25-321194725246",
        "SourceRepositoryName": "AE_Grooper"
    }

    base_node = {
        "Id": "f4d92776-554c-4633-ae37-e436e9fac79a",
        "Name": "GrooperGit",
        "TypeName": "Grooper.ObjectLibrary",
        "ParentId": "9bf35eff-6b29-48a6-a0fb-bd3ad72b97e9",
        "NodeIndex": 2
    }

    references = {}

    # Write JSON files
    with open(os.path.join(temp_directory, 'ArchiveInfo.json'), 'w') as f:
        json.dump(archive_info, f)
    with open(os.path.join(temp_directory, 'BaseNode.json'), 'w') as f:
        json.dump(base_node, f)
    with open(os.path.join(temp_directory, 'References.json'), 'w') as f:
        json.dump(references, f)


if __name__ == "__main__":
    parser = argparse.ArgumentParser(description="Rename files and generate JSON metadata.")
    parser.add_argument('--test', action='store_true', help="Run in test mode using a temporary directory.")
    args = parser.parse_args()

    src_directory = "GrooperGit"  # Adjust this path as necessary
    rename_files_and_generate_json(src_directory, use_temp_dir=args.test)

