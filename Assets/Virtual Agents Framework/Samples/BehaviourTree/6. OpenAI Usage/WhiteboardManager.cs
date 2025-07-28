using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace i5.VirtualAgents.AgentTasks
{
    public class WhiteboardManager : MonoBehaviour
    {
        public List<Sprite> sprites;

        public void ChangeSprite(string name)
        {
            var renderer = GetComponent<SpriteRenderer>();
            string name_without_ext = name.Split(".")[0];
            renderer.sprite = sprites.Find( (s) => s.name ==  name_without_ext);
        }

        // Start is called before the first frame update
        void Start()
        {
        
        }

        // Update is called once per frame
        void Update()
        {
        
        }
    }
}
