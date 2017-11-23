using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GrarphTutorial {
    class Container {
        public Guid uniqId = Guid.NewGuid();
        public string label;


        public Container(string l) {
            label = l;
        }

        public override bool Equals(object obj) {
            if(obj.GetType() == typeof(Container)){
                return ((Container)obj).uniqId.Equals(uniqId);
            }
            return base.Equals(obj);
        }

        public override int GetHashCode() {
            return uniqId.GetHashCode();
        }

        public override string ToString() {
            return label.ToString();
        }
    }
}
