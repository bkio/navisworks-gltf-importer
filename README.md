# Navisworks GLTF Importer
## Motivation and Quick Summary
It seems like Navisworks is lacking GLTF support at the moment. Thus, I created this small plugin to get GLTF files with metadata in the "extras" fields to be intermediately converted to FBX and a custom metadata binary format. Since Assimp library (it is the only free library which support GLTF import and export to a format that is supported by Navisworks. Paid option is Aspose.) unfortunately does not support metadata export feature, needed to create an intermediate binary format to populate hierarchy and metadata for each hierarchy node.

## What does it do?
Basically, provides a UI extension for user to pick up a GLTF file; takes care of the rest. Also during the process; it will automatically save NWD file in the same directory with chosen GLTF file.

## Performance?
Well, there are a lot of intermediate steps. 
- First convert GLTF to FBX; GLTF-Metadata to a custom binary format
- Read FBX in Navisworks
- Read custom binary format and populate each node in Navisworks scene with its metadata.
Therefore it is not a fast solution. But if you find any better solution, please let me know.

## How to install?
- Go to Releases section
- Download the Release_[version].zip you prefer.
- Unzip the zip file content to 
	- If you use Simulate C:\Program Files\Autodesk\Navisworks Simulate 2021\Plugins\GLTFImporterPlugin
	- If you use Manage C:\Program Files\Autodesk\Navisworks Manage 2021\Plugins\GLTFImporterPlugin

## How it works?
- Upon opening copying the Plugin to the destination as described above, and opening the Navisworks; there will be a tab at the top called "Tool add-ins 1".
- Click on "Tool add-ins 1" and there will be an option called "Import GLTF". Click on it and select the GLTF file. It will also import .bin files along with the main GLTF file.

## Compatibility?
- Plugin is tested on Navisworks Simulate 2021, have not tested on the other editions.
