using System;
using UnityEngine;


    [AttributeUsage(AttributeTargets.Class, Inherited = false, AllowMultiple = false)]
    public class DocsAttribute : PropertyAttribute
    {
        public string docsLink;

        public DocsAttribute(string link)
        {
            docsLink = link;
        }
    }
