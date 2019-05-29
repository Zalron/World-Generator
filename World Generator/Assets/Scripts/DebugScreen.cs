using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
namespace WorldGenerator
{
    public class DebugScreen : MonoBehaviour
    {
        public World world;
        public TextMeshProUGUI text;

        float frameRate;
        float timer;
        // Start is called before the first frame update
        void Start()
        {
            text = GameObject.Find("DebugScreen (TMP)").GetComponent<TextMeshProUGUI>();
        }

        // Update is called once per frame
        void Update()
        {
            string debugText = "World Generator";
            debugText += "\n";
            debugText += frameRate + " FPS";
            debugText += "\n\n";
            debugText += "XYZ: " + Mathf.FloorToInt(world.Player.transform.position.x) + " / " + Mathf.FloorToInt(world.Player.transform.position.y) + " / " + Mathf.FloorToInt(world.Player.transform.position.z);
            debugText += "\n";
            //debugText += "Chunk: " + world.playerChunkCoord.x + " / " + world.playerChunkCoord.y + " / " + world.playerChunkCoord.z;

            text.text = debugText;
            if (timer > 1f)
            {
                frameRate = (int)(1f / Time.unscaledDeltaTime);
                timer = 0;
            }
            else
            {
                timer += Time.deltaTime;
            }
        }
        
    }
}
