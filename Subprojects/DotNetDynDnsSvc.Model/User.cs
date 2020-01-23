using System.Collections.Generic;

namespace DotNetDynDnsSvc.Model
{
    public class User
    {
        public string username { get; set; }
        public string key { get; set; }
        public string resourceRecord { get; set; }
        public string zone { get; set; }
        public bool isAuthenticated { get; set; }
        public List<string> Actions { get; }

        public User()
        {
            username = "";
            key = "";
            resourceRecord = "";
            zone = "";
            isAuthenticated = false;
            Actions = new List<string>();
        }

        public void LoadActions(string actions)
        {
            //force everything to lowercase
            actions = actions.ToLower();

            Actions.Clear();
            //actions is either a single entry or a csv formatted line
            if (actions.Contains(","))
            {
                // string is csv formatted, split them and add each entry to the Actions list.
                string[] actionList = actions.Split(',');
                foreach (string action in actionList)
                {
                    Actions.Add(action);
                }
            }
            else
            {
                // actions is a single entry, add it to our actions list
                if (actions.Length > 0)
                    Actions.Add(actions);
            }
        }

        public bool IsPermitted(string action)
        {
            bool allowed = false;

            if (Actions.Contains(action.ToLower()))
                allowed = true;

            return allowed;
        }
    }
}