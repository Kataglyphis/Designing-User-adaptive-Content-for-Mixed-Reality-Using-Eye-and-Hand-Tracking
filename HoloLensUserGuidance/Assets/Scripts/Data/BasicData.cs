using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Assets.Scripts.Data
{
    public class BasicData : IDataClusterMetaInformation
    {
        public List<string> Description 
        {
            get
            {
                return new List<string>() { // UserId
                                            "Which user has HoloLens",
                                            // SessionType
                                            "f.e. which intention is tracked",
                                            // Timestamp
                                            "duration of sampling", };
            }

        }

        public List<string> Labels
        {
            get
            {
                return new List<string>() { 
                                            // UserId
                                            "UserId",
                                            // SessionType
                                            "SessionType",
                                            // Timestamp
                                            "dt in ms",
                };
            }

        }
    }
}
