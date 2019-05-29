using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
namespace WorldGenerator
{
    public class DebugScreen : MonoBehaviour
    {
        public Player player;
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
            int DebugPlayerChunkCoordx = world.playerChunkCoord.x;
            int DebugPlayerChunkCoordy = world.playerChunkCoord.y;
            int DebugPlayerChunkCoordz = world.playerChunkCoord.z;
            string debugText = "World Generator";
            debugText += "\n";
            debugText += frameRate + " FPS";
            debugText += "\n\n";
            debugText += "XYZ: " + Mathf.FloorToInt(world.Player.transform.position.x) + " / " + Mathf.FloorToInt(world.Player.transform.position.y) + " / " + Mathf.FloorToInt(world.Player.transform.position.z);
            debugText += "\n";
            debugText += "Chunk: " + DebugPlayerChunkCoordx + " / " + DebugPlayerChunkCoordy + " / " + DebugPlayerChunkCoordz;
            debugText += "\n";
            //debugText += "Block Selected" + player.;

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
