# Mobile Sink Wireless Sensor Network Simulator

 Tuft (Tree Based Heuristic Data Dissemination for Mobile Sink Wireless Sensor Networks)
 -----


To simulate Tuft, a new platform with a graphical user interface that provides details of simulations runs with other important pieces of information is developed.
The toolkit is developed using C# and WPF in .net 4.5.

Developed by Omar Busaileh et al. (busaileh@mail.ustc.edu.cn)

Implementation 
-----
Mobility model for the mobile sink is implemented in: https://github.com/howbani/tuft/tree/master/Models/MobileModel


Tuft-Cells structure is implemented in: https://github.com/howbani/tuft/tree/master/Constructor


Tuft’s designated node roles are implemented in: https://github.com/howbani/tuft/tree/master/Models/Cell

Bugs 
-----
If any bugs are encountered during the execution of the toolkit, please restart toolkit, or you can download the new version of this toolkit. If there is any error occurs, the toolkit will shut down automatically.

How to use the toolkit of simple testing
-----
1-	Open Tuft.exe in the Toolkit folder. If the toolkit can’t run pleases refer to (http://staff.ustc.edu.cn/~anmande/miniflow/#_Installation_Problems). 


2-	Import the required network topology, from File-> Import Topology.


3-	Set the mobile sink mean speed from the main menu Mobile Sink-> Set Mean Sink Speed, and set the desired speed.


4-	From the main menu, select Coverage->Random


5-	To select the rate of sending packet, from the main menu Test-> Select one Source per time, and select the desired rate.


How to use the toolkit for experiments and evaluation
-----


1-	Open Tuft.exe in the Toolkit folder. If the toolkit can’t run pleases refer to (http://staff.ustc.edu.cn/~anmande/miniflow/#_Installation_Problems). 


2-	Import the required network topology, from File-> Import Topology.


3-	Set the experiment settings, after clicking Experiment, and click Start.


4-	After the scenario is run, results are obtained from Show Results ->
