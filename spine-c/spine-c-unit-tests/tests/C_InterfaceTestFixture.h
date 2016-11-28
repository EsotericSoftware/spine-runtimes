#pragma once 
//////////////////////////////////////////////////////////////////////
//	filename: 	C_InterfaceTestFixture.h
//	
//	purpose:	Run example animations for regression testing
//				on "C" interface
/////////////////////////////////////////////////////////////////////

#include "TestOptions.h"
#include "MiniCppUnit.hxx"

class C_InterfaceTestFixture : public TestFixture<C_InterfaceTestFixture>
{
public:
	TEST_FIXTURE(C_InterfaceTestFixture)
	{
		// enable/disable individual tests here
		TEST_CASE(spineboyTestCase);
		TEST_CASE(raptorTestCase);
		TEST_CASE(goblinsTestCase);
	}

public:
	virtual void setUp();
	virtual void tearDown();

	void	spineboyTestCase();
	void	raptorTestCase();
	void	goblinsTestCase();
};
#if defined(gForceAllTests) || defined(gCInterfaceTestFixture)
REGISTER_FIXTURE(C_InterfaceTestFixture);
#endif