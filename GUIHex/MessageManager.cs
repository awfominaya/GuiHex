using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GUIHex
{
    class MessageManager
    {
        public static string GetMessages(string newmsg, string oldmsgs)
        {
            List<string> oldmessages = GetOldStringList(oldmsgs);
            List<string> templist = ManageListLength(oldmessages);
            return MakeMsgBoxMessages(templist, newmsg);
        }

        static List<string> GetOldStringList(string oldmsgs)
        {
            string[] oldmessages = oldmsgs.Split('\n');

            List<string> messages = new List<string>();
            foreach (string s in oldmessages)
            {
                if (s == "")
                {

                }
                else
                {
                    string[] split = s.Split('.');
                    messages.Add(split[1]);
                }
            }
            return messages;
        }

        static string MakeMsgBoxMessages(List<string> oldmessages, string newmsg)
        {
            List<string> flipped = new List<string>();
            flipped.Add(newmsg);

            for (int i = 0; i <oldmessages.Count; i++){
                flipped.Add(oldmessages[i]);
            }

            string concatmsg = "";

            int counter = 0;
            foreach(string e in flipped)
            {
                concatmsg += counter + "." + e + Environment.NewLine;
                counter++;
            }


            return concatmsg;
        }

        static List<string> ManageListLength(List<string> oldmsgs)
        {
            List<string> templist = new List<string>(oldmsgs);

            int counter = 0;
            while (oldmsgs.Count >= 49)
            {
                templist.Remove(templist[counter]);
            }

            List<string> returnlist = new List<string>();

            foreach(string e in templist)
            {
                returnlist.Add(e);
            }

            return returnlist;
        }

    }
}
