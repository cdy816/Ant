using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InAntStudio.ViewModel
{
    /// <summary>
    /// 
    /// </summary>
    public  class ScriptNewViewModel:WindowViewModelBase
    {
        private string mName;
        private string mDescription;
        /// <summary>
        /// 
        /// </summary>
        public ScriptNewViewModel()
        {
            DefaultHeight = 80;
            DefaultWidth = 400;
            Title = Res.Get("NewScript");
        }

        /// <summary>
        /// 
        /// </summary>
        public string Name { get { return mName; } set { mName=value;OnPropertyChanged("Name"); } }

        /// <summary>
        /// 
        /// </summary>
        public string Description { get { return mDescription; } set { mDescription = value; OnPropertyChanged("Description"); } }

    }
}
