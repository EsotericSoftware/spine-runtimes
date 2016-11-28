#pragma once 
#include "MiniCppUnit.hxx"

class [[FIXTURE_TYPE]] : public TestFixture < [[FIXTURE_TYPE]] >
{
public:
	TEST_FIXTURE([[FIXTURE_TYPE]]){
		//TEST_CASE(parseJSON);

		initialize();
	}

	virtual ~[[FIXTURE_TYPE]]();

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
REGISTER_FIXTURE([[FIXTURE_TYPE]]);