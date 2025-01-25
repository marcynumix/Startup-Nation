#This is an example that uses the websockets api to know when a prompt execution is done
#Once the prompt execution is done it downloads the images using the /history endpoint

import websocket #NOTE: websocket-client (https://github.com/websocket-client/websocket-client)
import uuid
import json
import urllib.request
import urllib.parse
import sys

# server_address = "192.168.1.72:8188"
# get server adress from arguments
server_address = sys.argv[1]
client_id = str(uuid.uuid4())

def queue_prompt(prompt):
    p = {"prompt": prompt, "client_id": client_id}
    data = json.dumps(p).encode('utf-8')
    req =  urllib.request.Request("http://{}/prompt".format(server_address), data=data)
    return json.loads(urllib.request.urlopen(req).read())

def get_image(filename, subfolder, folder_type):
    data = {"filename": filename, "subfolder": subfolder, "type": folder_type}
    url_values = urllib.parse.urlencode(data)
    with urllib.request.urlopen("http://{}/view?{}".format(server_address, url_values)) as response:
        return response.read()

def get_history(prompt_id):
    with urllib.request.urlopen("http://{}/history/{}".format(server_address, prompt_id)) as response:
        return json.loads(response.read())

def get_images(ws, prompt):
    prompt_id = queue_prompt(prompt)['prompt_id']
    output_images = {}
    while True:
        out = ws.recv()
        if isinstance(out, str):
            message = json.loads(out)
            if message['type'] == 'executing':
                data = message['data']
                if data['node'] is None and data['prompt_id'] == prompt_id:
                    break #Execution is done
        else:
            # If you want to be able to decode the binary stream for latent previews, here is how you can do it:
            # bytesIO = BytesIO(out[8:])
            # preview_image = Image.open(bytesIO) # This is your preview in PIL image format, store it in a global
            continue #previews are binary data

    history = get_history(prompt_id)[prompt_id]
    for node_id in history['outputs']:
        node_output = history['outputs'][node_id]
        images_output = []
        if 'images' in node_output:
            for image in node_output['images']:
                image_data = get_image(image['filename'], image['subfolder'], image['type'])
                images_output.append(image_data)
        output_images[node_id] = images_output

    return output_images

prompt_text = """
{
  "4": {
    "inputs": {
      "ckpt_name": "sd3.5_medium.safetensors"
    },
    "class_type": "CheckpointLoaderSimple",
    "_meta": {
      "title": "Load Checkpoint"
    }
  },
  "6": {
    "inputs": {
      "text": "Create an illustration of a startup concept called 'Growtivate: The Motivational Plant.' This smart plant yells at you if you procrastinate by using sensors to detect if you spend too much time sitting in front of your screen. The image should depict a vibrant, futuristic indoor setting with a quirky, high-tech plant that has built-in speakers and motion sensors. The plant should have an expressive, slightly aggressive personality, with visual elements like flashing lights, speech bubbles, or an animated face to convey its motivation tactics. The atmosphere should be humorous yet modern, combining productivity and technology aesthetics.",
      "clip": [
        "11",
        0
      ]
    },
    "class_type": "CLIPTextEncode",
    "_meta": {
      "title": "CLIP Text Encode (Prompt)"
    }
  },
  "8": {
    "inputs": {
      "samples": [
        "294",
        0
      ],
      "vae": [
        "4",
        2
      ]
    },
    "class_type": "VAEDecode",
    "_meta": {
      "title": "VAE Decode"
    }
  },
  "11": {
    "inputs": {
      "clip_name1": "clip_g.safetensors",
      "clip_name2": "clip_l.safetensors",
      "clip_name3": "t5xxl_fp8_e4m3fn.safetensors"
    },
    "class_type": "TripleCLIPLoader",
    "_meta": {
      "title": "TripleCLIPLoader"
    }
  },
  "13": {
    "inputs": {
      "shift": 3,
      "model": [
        "4",
        0
      ]
    },
    "class_type": "ModelSamplingSD3",
    "_meta": {
      "title": "ModelSamplingSD3"
    }
  },
  "50": {
    "inputs": {
      "images": [
        "8",
        0
      ]
    },
    "class_type": "PreviewImage",
    "_meta": {
      "title": "Preview Image"
    }
  },
  "67": {
    "inputs": {
      "conditioning": [
        "71",
        0
      ]
    },
    "class_type": "ConditioningZeroOut",
    "_meta": {
      "title": "ConditioningZeroOut"
    }
  },
  "68": {
    "inputs": {
      "start": 0.2,
      "end": 1,
      "conditioning": [
        "67",
        0
      ]
    },
    "class_type": "ConditioningSetTimestepRange",
    "_meta": {
      "title": "ConditioningSetTimestepRange"
    }
  },
  "69": {
    "inputs": {
      "conditioning_1": [
        "68",
        0
      ],
      "conditioning_2": [
        "70",
        0
      ]
    },
    "class_type": "ConditioningCombine",
    "_meta": {
      "title": "Conditioning (Combine)"
    }
  },
  "70": {
    "inputs": {
      "start": 0,
      "end": 0.2,
      "conditioning": [
        "71",
        0
      ]
    },
    "class_type": "ConditioningSetTimestepRange",
    "_meta": {
      "title": "ConditioningSetTimestepRange"
    }
  },
  "71": {
    "inputs": {
      "text": "",
      "clip": [
        "11",
        0
      ]
    },
    "class_type": "CLIPTextEncode",
    "_meta": {
      "title": "CLIP Text Encode (Prompt)"
    }
  },
  "135": {
    "inputs": {
      "width": 512,
      "height": 512,
      "batch_size": 1
    },
    "class_type": "EmptySD3LatentImage",
    "_meta": {
      "title": "EmptySD3LatentImage"
    }
  },
  "294": {
    "inputs": {
      "seed": 78556820381604,
      "steps": 40,
      "cfg": 4.5,
      "sampler_name": "dpmpp_2m",
      "scheduler": "sgm_uniform",
      "denoise": 1,
      "model": [
        "13",
        0
      ],
      "positive": [
        "6",
        0
      ],
      "negative": [
        "69",
        0
      ],
      "latent_image": [
        "135",
        0
      ]
    },
    "class_type": "KSampler",
    "_meta": {
      "title": "KSampler"
    }
  }
}
"""

prompt = json.loads(prompt_text)
#set the text prompt for our positive CLIPTextEncode
#prompt["6"]["inputs"]["text"] = "masterpiece best quality man"

#get the prompt from the command line
prompt["6"]["inputs"]["text"] = sys.argv[2]

#set the seed for our KSampler node
#prompt["3"]["inputs"]["seed"] = 5

ws = websocket.WebSocket()
ws.connect("ws://{}/ws?clientId={}".format(server_address, client_id))
images = get_images(ws, prompt)
ws.close() # for in case this example is used in an environment where it will be repeatedly called, like in a Gradio app. otherwise, you'll randomly receive connection timeouts
#Commented out code to display the output images:

# Save images to disk
for node_id in images:
    for i, image_data in enumerate(images[node_id]):
        with open(f"output_{node_id}_{i}.png", "wb") as f:
            f.write(image_data)

            
# for node_id in images:
#     for image_data in images[node_id]:
#         from PIL import Image
#         import io
#         image = Image.open(io.BytesIO(image_data))
#         image.show()

