using System.Collections.Generic;

namespace ExtensionMethods
{
    public static class ListExtensions
    {
        public static string ToStringMembers<T>(this List<T> list)
        {
            string membersString = "[";
            foreach (T ListMember in list)
            {
                membersString += ListMember.ToString() + ", ";
            }
            membersString += "]";
            return membersString;
        }
    }
}