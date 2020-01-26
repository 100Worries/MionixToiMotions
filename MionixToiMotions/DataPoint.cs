using System;
using System.Runtime.Serialization;

namespace MionixToiMotions
{
    //DataContract for Serializing Data - required to serve in JSON format
    [DataContract]
    public class DataPoint
    {
        public DataPoint(float heartRate, float gsr)
        {
            this.hRate = heartRate;
            this.gsrLvl = gsr;
        }

        //Explicitly setting the name to be used while serializing to JSON.
        [DataMember(Name = "Heart Rate")]
        public Nullable<float> hRate = null;

        //Explicitly setting the name to be used while serializing to JSON.
        [DataMember(Name = "GSR")]
        public Nullable<float> gsrLvl = null;
    }
}
