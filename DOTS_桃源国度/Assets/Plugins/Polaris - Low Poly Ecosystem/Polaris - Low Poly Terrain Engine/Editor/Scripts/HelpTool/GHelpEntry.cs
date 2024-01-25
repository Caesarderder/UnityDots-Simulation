#if GRIFFIN
using System.Collections.Generic;
using UnityEngine;

namespace Pinwheel.Griffin.HelpTool
{
    [System.Serializable]
    public struct GHelpEntry
    {
        [SerializeField]
        private int id;
        public int Id
        {
            get
            {
                return id;
            }
            set
            {
                id = Mathf.Max(1, value);
            }
        }

        [SerializeField]
        private GCategory category;
        public GCategory Category
        {
            get
            {
                return category;
            }
            set
            {
                category = value;
            }
        }

        [SerializeField]
        private string question;
        public string Question
        {
            get
            {
                return question;
            }
            set
            {
                question = value;
            }
        }

        [SerializeField]
        private string answer;
        public string Answer
        {
            get
            {
                return answer;
            }
            set
            {
                answer = value;
            }
        }

        [SerializeField]
        private List<GHelpLink> links;
        public List<GHelpLink> Links
        {
            get
            {
                if (links == null)
                {
                    links = new List<GHelpLink>();
                }
                return links;
            }
            set
            {
                links = value;
            }
        }
    }
}
#endif
