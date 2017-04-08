#pragma once

//////////////////////////////////////////////////////////////////////////
// Force all Tests to 'ON.'  Use this for final 'Regression' Testing.

//#define gForceAllTests

//#define TURN_ON_ALL_TESTS // Comment this line out to switch to fast testing only

#ifdef TURN_ON_ALL_TESTS
//////////////////////////////////////////////////////////////////////////
// All tests are ON by default, but you can turn off individual tests.

#define gEmptyTestFixture
#define gCInterfaceTestFixture
#define gCPPInterfaceTestFixture
#define gMemoryTestFixture


#else

//////////////////////////////////////////////////////////////////////////
// Slow Tests are disabled by default.  Use this section to turn on
// Individual tests.
#define gEmptyTestFixture  // Fast

#define gCInterfaceTestFixture // slow
#define gCPPInterfaceTestFixture // fast

#define gMemoryTestFixture // medium

#endif