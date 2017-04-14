#pragma once 
#include "MiniCppUnit.hxx"

class CPP_InterfaceTestFixture : public TestFixture < CPP_InterfaceTestFixture >
{
public:
	TEST_FIXTURE(CPP_InterfaceTestFixture){
		//TEST_CASE(parseJSON);

		initialize();
	}

	virtual ~CPP_InterfaceTestFixture();

	//////////////////////////////////////////////////////////////////////////
	// Test Cases
	//////////////////////////////////////////////////////////////////////////
public:
	// void parseJSON();

	//////////////////////////////////////////////////////////////////////////
	// test fixture setup
	//////////////////////////////////////////////////////////////////////////
	void initialize();
	void finalize();
public:
	virtual void setUp();
	virtual void tearDown();
};
REGISTER_FIXTURE(CPP_InterfaceTestFixture);