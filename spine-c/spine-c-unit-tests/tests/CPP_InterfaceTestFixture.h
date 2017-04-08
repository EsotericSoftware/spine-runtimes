#pragma once 
//////////////////////////////////////////////////////////////////////
//	filename: 	C_InterfaceTestFixture.h
//	
//	purpose:	Run example animations for regression testing
//				on "C++" interface to make sure modifications to "C"
//				interface doesn't cause memory leaks or regression 
//				errors.
/////////////////////////////////////////////////////////////////////

#include "MiniCppUnit.hxx"
#include "TestOptions.h"

class CPP_InterfaceTestFixture : public TestFixture < CPP_InterfaceTestFixture >
{
public:
	TEST_FIXTURE(CPP_InterfaceTestFixture){
		TEST_CASE(spineboyTestCase);
		TEST_CASE(raptorTestCase);
		TEST_CASE(goblinsTestCase);

		initialize();
	}

	virtual ~CPP_InterfaceTestFixture();

	//////////////////////////////////////////////////////////////////////////
	// Test Cases
	//////////////////////////////////////////////////////////////////////////
public:
	void	spineboyTestCase();
	void	raptorTestCase();
	void	goblinsTestCase();

	//////////////////////////////////////////////////////////////////////////
	// test fixture setup
	//////////////////////////////////////////////////////////////////////////
	void initialize();
	void finalize();
public:
	virtual void setUp();
	virtual void tearDown();

};
#if defined(gForceAllTests) || defined(gCPPInterfaceTestFixture)
REGISTER_FIXTURE(CPP_InterfaceTestFixture);
#endif
