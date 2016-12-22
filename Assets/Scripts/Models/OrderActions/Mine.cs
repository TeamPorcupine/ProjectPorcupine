#region License
// ====================================================
// Project Porcupine Copyright(C) 2016 Team Porcupine
// This program comes with ABSOLUTELY NO WARRANTY; This is free software, 
// and you are welcome to redistribute it under certain conditions; See 
// file LICENSE, which is part of this source code package, for details.
// ====================================================
#endregion
using System;
using System.Xml.Serialization;

namespace ProjectPorcupine.OrderActions
{
    [Serializable]
    [XmlRoot("OrderAction")]
    [OrderActionName("Mine")]
    public class Mine : OrderAction
    {
        public Mine()
        {
        }

        private Mine(Mine other) : base(other)
        {
            JobInfo = other.JobInfo;
        }

        [XmlElement("Job")]
        public JobInformation JobInfo { get; set; }

        public override void Initialize(string type)
        {
            base.Initialize(type);

            // if there is no JobInfo defined, use defaults (time=0, ...)
            if (JobInfo == null)
            {
                JobInfo = new JobInformation();
            }
        }

        public override OrderAction Clone()
        {
            return new Mine(this);
        }

        public override Job CreateJob(Tile tile, string type)
        {
            Job job = CheckJobFromFunction(JobInfo.FromFunction, tile.Furniture);

            return job;
        }
    }
}
