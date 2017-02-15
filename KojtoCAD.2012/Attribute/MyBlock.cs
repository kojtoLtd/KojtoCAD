using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Text;

namespace KojtoCAD.Attribute
{
    public class MyBlock
    {
        public string Blockname = "";
        public StringCollection Attributes = new StringCollection();
        public MyBlock()
        {
            Blockname = "";
            Attributes = new StringCollection();
        }
        public MyBlock(string name)
        {
            Blockname = name;
            Attributes = new StringCollection();
        }
        public void AddAtt(string tag)
        {
            Attributes.Add(tag);
        }
        public string GetAtt(int i)
        {
            string rez = "-1";
            if ( ( i >= 0 ) && ( i < Attributes.Count ) )
            {
                rez = Attributes[i];
            }
            return rez;
        }
        public int Find(string tag)
        {
            int rez = -1;
            for ( int i = 0; i < Attributes.Count; i++ )
            {
                if ( tag.ToUpper() == Attributes[i].ToUpper() )
                {
                    rez = i;
                    break;
                }
            }
            return rez;
        }
    }
}
