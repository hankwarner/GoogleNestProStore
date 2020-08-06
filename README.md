# NestProOrderImporter
A scheduled console application for importing orders from the Google Nest Pro store (hosted by Big Commerce) into NetSuite for fulfillment and reporting.
The job is scheduled on SQL server 02 (Athena) to run every 15 minutes. The program uses the Big Commerce API to retrieve new orders from the Nest Pro store
and sends the order information to NetSuite to generate a sales order and cash sale.



## Program Workflow
* Call the Big Commerce API to get all orders in _Awaiting Fulfillment_ status. This status flags new orders that have not imported to NetSuite.

* Call the Big Commerce API to get the shipping information on the order.

* For all orders that need to be imported, send the customer information to the CreateCustomerRESTlet to get the customer's NetSuite ID.

* Call the Big Commerce API to get the all products on the order. _Note: if the item has a Pack Quantity, this integer should be appended to the end of the SKU, 
prefixed by an underscore. Example: TS3019US_4_.

* Get the NetSuite internal ID for each product on the order from the database.

* Map the required fields from Big Commerce to the expected NetSuite values and send the request to NetSuite to create the sales order.

* Store the NetSuite sales order internal ID in the _staff_notes_ field, and move the order status to _Awaiting Shipment_.


## Notable Field Mappings
`SiteOrderNumber` is the Big Commerce order ID prefixed with `NP`.

`AltOrderNumber` is the `payment_provider_id` provided by Big Commerce.

The NetSuite sales order ID is written back to the Big Commerce order and stored in the `staff_notes` field.


### Default fields
`Microsite` is 31 (Nest Pro).

`CheckoutTypeId` is 4 (registered).

`PaymentMethodId` is 1 (credit card).

`SameDayShipping` is 3 (no-fully committed only).

`UserTypeId` is 4 (general contractor).

`Department` is 29 (Pro Sales).



## NetSuite Scripts
**CreateCustomerRESTlet** (_script id 1762_) - sends a JSON request with the purchasing customer's information to retrive
the internal NetSuite ID of the customer. 


Example request:
```
{
   "Email": "BarryBlock@CousineauActingStudio.com",
   "BillingFirstName": "Barry",
   "BillingLastName": "Block",
   "Department": "29",
   "UserTypeId": "4",
   "PhoneNumber": "7064642574",
   "Company": "Gene Cousineau's Acting Studio",
   "SameDayShipping": "2",
   "BillingLine1:" "311 Amber Lane",
   "BillingLine2": "Apt B",
   "BillingCity": "Ventura",
   "BillingState": "CA",
   "BillingZip": "90754",
   "ShippingFirstName": "Sally",
   "ShippingLastName": "Reed",
   "ShippingLine1": "141 Tupelo Dr.",
   "ShippingLine2": "Unit 605",
   "ShippingCity": "Santa Monica",
   "ShippingState": "CA",
   "ShippingZip": "91578"
}
```



**WebsiteOrderImporterRESTlet** (_script id 1761_) - sends a JSON request that will create the sales order record in NetSuite. 
The Processing Gateway ID is included in the request, which will trigger a cash sale record to be created along with the sales order.


Example request:
```
{
	"CustomerId": 17494445,
	"SiteOrderNumber": "897654654",
	"AltOrderNumber": "61954029542",
	"Email": "BarryBlock@GeneCousineauActingStudio.com",
	"PhoneNumber": "214-264-6874",
	"BillingFirstName": "Barry",
	"BillingLastName": "Block",
	"BillingLine1": "7219 Centenary Ave",
	"BillingLine2": "Unit A",
	"BillingCity": "Los Angeles",
	"BillingState": "CA",
	"BillingZip": "90066",
	"BillingCountry": "US",
	"ShippingFirstName": "Gene",
	"ShippingLastName": "Parmesan",
	"ShippingLine1": "311 Amber Lane",
	"ShippingLine2": "Apt B",
	"ShippingCity": "Ventura",
	"ShippingState": "CA",
	"ShippingZip": "93001",
	"ShippingCountry": "US",
	"ShippingPhone": "212-974-4854",
	"Note": "This is a valuable customer",
	"SH": 50.00,
	"ShippingMethodName": "UPS 2nd Day Air Early A.M.",
	"JobName": "Studio construction",
	"IPAddress": "99.203.23.226",
	"Microsite": 27,
	"Department": 29,
	"UserTypeId": 2,
	"CheckoutTypeId": 4,
	"PaymentMethodId": 1,
	"Items": [
		{
			"ItemId": "10268",
			"Quantity": 2,
			"Rate": 120.00,
			"Amount": 240.00,
			"PersonalItem": false
		},
		{
			"ItemId": "78945",
			"Quantity": 3,
			"Rate": 100.00,
			"Amount": 300.00,
			"PersonalItem": true
		}
	]
}
```



## Nuget Packages
RestSharp - simple REST and HTTP API client

Dapper - a high performance Micro-ORM supporting SQL Server, MySQL, Sqlite, SqlCE, Firebird, etc.

Newtonsoft.Json - a high performance JSON framework for .NET

Serilog - simple .NET logging with fully-structured events.

xUnit - a developer testing framework, built to support Test Driven Development.





# NestProShipments
A scheduled console application for importing shipments (item fulfillments) from NetSuite to the Google Nest Pro store (hosted by Big Commerce). 
The job is scheduled on SQL server 02 (Athena) to run every 15 minutes. The program retrieves new item fulfillment records from the NetSuite database
that need to be imported into Big Commerce so the customer will receive an email with the tracking number(s) and fulfillment information.



## Program Workflow
* Call the Big Commerce API to get all orders in _Awaiting Fulfillment_ and _Partially Fulfilled_ statuses. 

* For all item fulfillments that need to be imported to Big Commerce, get the sales order ID from the _staff_notes_ field. This is used in the query that will 
return item fulfillments for the related sales order ID. 

* For orders in _Partially Shipped_ status, get the item fulfillment IDs from the `comments` field on the shipment and exclude these shipments since they already exist in Big Commerce.

* Generate the shipment request and create the new shipment via the Big Comemrce API. _Note: for orders with kits, Big Commerce uses the kit quantity rather than the individual item quantities like NetSuite does, 
so we must parse the kit pack quantity to get the number of kits._

* Note on tracking numbers: for item fulfillments in NetSuite, it is possible to have more than one tracking number. However, Big Commerece has a limit on tracking numbers on a shipment to 2. 
Therefore, the program will only send the first two tracking numbers on an individual item fulfillment record.



## Nuget Packages
RestSharp - simple REST and HTTP API client

Dapper - a high performance Micro-ORM supporting SQL Server, MySQL, Sqlite, SqlCE, Firebird, etc.

Newtonsoft.Json - a high performance JSON framework for .NET

Serilog - simple .NET logging with fully-structured events.

xUnit - a developer testing framework, built to support Test Driven Development.
