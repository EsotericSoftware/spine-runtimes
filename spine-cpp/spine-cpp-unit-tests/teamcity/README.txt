CppUnit listener for TeamCity
-----------------------------

To report your tests result to TeamCity server
include teamcity_messages.* teamcity_cppunit.*
to your project and modify "main" function
as shown in example.cpp
(around JetBrains::underTeamcity and JetBrains::TeamcityProgressListener)

Technical details
-----------------

Reporting implemented by writing TeamCity service messages to stdout.

See
http://www.jetbrains.net/confluence/display/TCD3/Build+Script+Interaction+with+TeamCity
for more details.

Contact information
-------------------

Mail to teamcity-feedback@jetbrains.com or see other options at

http://www.jetbrains.com/support/teamcity

License
-------

Apache, version 2.0
http://www.apache.org/licenses/LICENSE-2.0
