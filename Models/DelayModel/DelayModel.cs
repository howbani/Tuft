﻿using Tuft.Intilization;
using Tuft.Dataplane;

namespace Tuft.DelayModel
{

    // FOR CODE SEE:http://ns2.sourcearchive.com/documentation/2.35~RC4-1/threshold_8cc-source.html
    /// <summary>
    /// https://en.wikipedia.org/wiki/Packet_transfer_delay
    /// https://en.wikipedia.org/wiki/End-to-end_delay
    /// End-to-end delay
    /// End-to-end delay or one-way delay refers to the time taken for
    /// a packet to be transmitted across a network from source to destination. 
    /// It is a common term in IP network monitoring, and differs from Round-Trip Time (RTT)
    /// </summary>
    public class DelayModel
    {
        /// <summary>
        ///  get the delay: TransmissionDelay + PropagationDelay;
        /// </summary>
        /// <param name="tx"></param>
        /// <param name="rc"></param>
        /// <returns></returns>
        public static double Delay(Sensor tx, Sensor rc)
        {
            double Distance = Operations.DistanceBetweenTwoSensors(tx, rc);
            //https://en.wikipedia.org/wiki/Transmission_delay
            double TransmissionDelay = PublicParameters.RoutingDataLength / PublicParameters.TransmissionRate;

            //en.wikipedia.org/wiki/Propagation_delay

            double PropagationDelay = Distance / PublicParameters.SpeedOfLight;
            return TransmissionDelay + PropagationDelay;
        }
    }
}
