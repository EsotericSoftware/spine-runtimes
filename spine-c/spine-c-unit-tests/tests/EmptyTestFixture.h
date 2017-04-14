#pragma once 
#include "TestOptions.h"
#include "MiniCppUnit.hxx"

class EmptyTestFixture : public TestFixture<EmptyTestFixture>
{
public:
	TEST_FIXTURE(EmptyTestFixture)
	{
		// enable/disable individual tests here
		TEST_CASE(emptyTestCase_1);
		TEST_CASE(emptyTestCase_2);
		TEST_CASE(emptyTestCase_3);
	}

public:
	virtual void setUp();
	virtual void tearDown();

	void	emptyTestCase_1();
	void	emptyTestCase_2();
	void	emptyTestCase_3();
};
#if defined(gForceAllTests) || defined(gEmptyTestFixture)
REGISTER_FIXTURE(EmptyTestFixture);
#endif