using Assets.Scripts.ModelsReform;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Assets.Scripts.Info
{
    public class ConfigurationInfo
    {
        public ConfigurationInfo(ConfigurationData confData)
        {
            projectName = confData.PROJECT_NAME;
            titleName = confData.TITLE_NAME;
        }
        public string projectName;
        public string titleName;
    }
}
