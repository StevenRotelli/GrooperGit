import os
import json
import uuid
from datetime import datetime

def generate_guid():
    return str(uuid.uuid4())

def current_datetime():
    # Get the current date and time in the specified format
    return datetime.now().astimezone().isoformat()

def rename_files_and_generate_json(src_directory):
    files_mapping = []
    for root, dirs, files in os.walk(src_directory):
        for name in files:
            original_path = os.path.join(root, name)
            new_name = f"{generate_guid()}_{name}.grp"
            new_path = os.path.join(root, new_name)
            os.rename(original_path, new_path)
            files_mapping.append({"original": name, "new": new_name})

    archive_info = {
        "Author": "Steven T. Rotelli",
        "Publisher": "BIS",
        "ArchiveDate": current_datetime(),  # Use the current date and time
        "Version": "23.1.0012",
        "Name": "GrooperThemes",
        "BaseNodeId": "f4d92776-554c-4633-ae37-e436e9fac79a",
        "SourceRepositoryId": "63193412-42d3-4922-9c25-321194725246",
        "SourceRepositoryName": "AE_Grooper"
    }

    base_node = {
        "Id": "f4d92776-554c-4633-ae37-e436e9fac79a",
        "Name": "GrooperThemes",
        "TypeName": "Grooper.ObjectLibrary",
        "ParentId": "9bf35eff-6b29-48a6-a0fb-bd3ad72b97e9",
        "NodeIndex": 2
    }

    references = {}

    # Write JSON files
    with open(os.path.join(src_directory, 'ArchiveInfo.json'), 'w') as f:
        json.dump(archive_info, f)
    with open(os.path.join(src_directory, 'BaseNode.json'), 'w') as f:
        json.dump(base_node, f)
    with open(os.path.join(src_directory, 'References.json'), 'w') as f:
        json.dump(references, f)

if __name__ == "__main__":
    src_directory = "GrooperThemes" # Adjust this path as necessary
    rename_files_and_generate_json(src_directory)
