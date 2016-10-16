Mock FIX Trading Server and AT Order Book Client by Heathmill Ltd
=================================================================
A demonstration FIX trading system created by Heathmill Ltd.

Mock FIX Server
---------------
A Mock FIX trading server suitable for testing FIX trading clients against.

* Accepts connections from FIX clients
* Supports Good-til-Cancelled Limit orders
* Auto-matches orders to execute trades
* Sends execution reports for orders and trades to connected clients
* Supports FIX 4.2 and FIX 4.4 sessions

See the Code Project article for more information:
http://www.codeproject.com/Articles/757708/Mock-FIX-Trading-Server

AT Order Book Client
--------------------
A demo FIX UI client that can create synthetic orders based on Automated Trading (AT) rules.

* Connects using FIX 4.4 to a FIX trading server
    * In this case the Mock FIX server above
* Displays the orders on the server as sorted order stacks for each contract
* Submits Limit orders to the server
* Allows click-trading of orders
* Create, activate and suspend Iceberg automated trading synthetic orders


See the Code Project article for more information:
http://www.codeproject.com/Articles/782560/WPF-FIX-Automated-Trading-Client


To build the solution
----------------------
* If you've not used NuGet before then install NuGet using the Visual Studio Extension Manager (via the Tools menu or via http://visualstudiogallery.msdn.microsoft.com/27077b70-9dad-4c64-adcf-c7cf6bc9970c)
* Restore the NuGet packages:
    * Right-click on the Solution in Solution Explorer and choose "Enable NuGet Package Restore"
        * This will create a .nuget directory under the solution directory
    * Right-click on the Solution in Solution Explorer and choose "Manage NuGet Packages for Solution ..."
    * Click "Restore" in the top-right corner
    * See the page on NuGet Package Restore for more info (https://docs.nuget.org/docs/reference/package-restore)
* Build the solution as usual