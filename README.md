# RealAntennas
KSP Mod to add better antenna / link calculations to CommNet.  Extends most CommNet classes.

First, thanks to TaxiService; CommNetConstellation (https://github.com/KSP-TaxiService/CommNetConstellation) was invaluable in getting started with modifying CommNet.

The primary driver for this mod is to replace the KSP notion of an individual antenna having a "range" as a singular value that presumably derives from its gain and its transmission power.  KSP's stock CommNet exposes a replacable RangeModel at its top level Scenario, but unfortunately the parameters of the functions defined in this interface only contain the "range" doubles and the distance double.  Direct access to the CommNode object, and a method to control the object's class when it is created [in several different places] would have made this mod MUCH simpler.

First, this mod implements a new interface in place of the RangeModel, which operates on extended CommNode objects in CommNetwork.  The new objects have a different version of the antenna info, with characteristics for antenna gain, transmit power, coding gain, noise figure [and a few others].  It implements a typical link budget calculation:  RxPower = TxPower + TxGain - FreeSpacePathLoss + RxGain + CodingGain.  Then, C/I or SNR = RxPower - Rx_NoiseFloor.

Note there are some simplifications to get started.  We're setting the noise temperature of the antenna to 290K (as if it's always an omni on Earth).  Bandwidth is applied to total noise power.  We aren't implementing other path loss effects (atmospheric, or edge-diffraction).  We aren't implementing pointing loss, and are not enforcing that directional antennas are directional ala RT.  CodingGain is currently just a fixed value, versus a scalable factor to trade data rate for SNR.  We have a naive notion of frequency that is applied to the path loss calculation properly.  Adjusting these are on the TODO, but I needed to get something up for review.

Second, this mod manipulates how CommNetVessels select the best antenna for a given link.  The current method is very simplistic: max (txPower + gain).  It might be better to defer the antenna selection until in TryConnect(), where antennas can be selected by some configurable paradigm: best receiver, best transmitter, best data rate, other?

Third, this mod manipulates SetNodeConnection() and TryConnect() in CommNetwork.  This invokes our replacement range model, and then sets the link characteristics displayed by CommNet based on the C/I calculation.  Now that we have access to the CommNode objects, with all of the underlying antenna info instead of the simplified "range" double, we can implement our chosen formulas.

We create from scratch CommNetHome and CommNetBody objects rather than replacing CommNet stock's.  We borrowed RemoteTech's ConfigNode structure.  We modified these primarily to get access to the methods that created CommNodes so we could replace them with our custom class.

Future work is to move beyond the basic link budget calculation and start affecting data transmission rates, and implement variable coding schemes.  I don't think I have much talent for UI, so that will need some help.  CommNet links are assumed bidirectional, so when establishing them we take the worst result for each direction.  Value in implementing asymmetric communications links is TBD.  There's not much concept for uplink data rate beyond command and control signaling, but is there much purpose to uplink C2 if there is no telemetry/state data from the remote end?

Reference materiel:

https://forum.kerbalspaceprogram.com/index.php?/topic/156251-commnet-notes-for-modders/

https://en.wikipedia.org/wiki/Johnson%E2%80%93Nyquist_noise

https://en.wikipedia.org/wiki/Noise_temperature

http://www.delmarnorth.com/microwave/requirements/satellite_noise.pdf

https://www.itu.int/dms_pubrec/itu-r/rec/p/R-REC-P.372-7-200102-S!!PDF-E.pdf
