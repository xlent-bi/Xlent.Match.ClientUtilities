# ClientUtilities release history

## 3.3 (2014-12-14)

* NEW: Added a public static method in Data to calculate a checksum for properties.
* NEW: Added a Checksum field for the MatchObject data. This value is stored in the Key table.
* NEW: AdapterSubscription now has automatic check of the Checksum and takes action.
* NEW: MatchEngine now accepts FailureResponse caused by Checksum error and takes the appropriate action.
* CHANGE: All configured names are now case insensitive. Match still knows and uses the correct case sensitive names, but cases are not considered relevant from users/adapters/service callers.* CHANGE: Setting IsClientUpdated or IsMdUpdated to 1 now means setting HasError=0 and Status=Normal

## 3.2 (2014-09-15)

* CHANGE: Now uses the same version numbers as Match. Can be abandoned later, but makes sense now when Match is where we do the major part of the development right now.
* CHANGE: Now uses async as much as possible for Service Bus.

## 1.1 (2014-08-11)

* CHANGE: The AdapterSubscription now has ReceiveMode set to ReceiveAndDelete.

