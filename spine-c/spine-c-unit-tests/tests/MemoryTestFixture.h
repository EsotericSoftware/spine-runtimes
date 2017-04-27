//////////////////////////////////////////////////////////////////////
//	filename: 	MemoryTestFixture.h
//	
//	purpose:	Reproduce Memory Error/Leak Bugs to help debug
//				and for regression testing
/////////////////////////////////////////////////////////////////////

#pragma once 
#include "MiniCppUnit.hxx"
#include "TestOptions.h"

class MemoryTestFixture : public TestFixture < MemoryTestFixture >
{
public:
	TEST_FIXTURE(MemoryTestFixture){

		// Comment out here to disable individual test cases 
		TEST_CASE(reproduceIssue_776);
		TEST_CASE(reproduceIssue_777);
		TEST_CASE(reproduceIssue_Loop);
		TEST_CASE(triangulator);
		TEST_CASE(skeletonClipper);

		initialize();
	}

	virtual ~MemoryTestFixture();

	//////////////////////////////////////////////////////////////////////////
	// Test Cases
	//////////////////////////////////////////////////////////////////////////
public:
	void reproduceIssue_776();
	void reproduceIssue_777();
	void reproduceIssue_Loop(); // http://esotericsoftware.com/forum/spine-c-3-5-animation-jerking-7451
	void triangulator();
	void skeletonClipper();

	//////////////////////////////////////////////////////////////////////////
	// test fixture setup
	//////////////////////////////////////////////////////////////////////////
	void initialize();
	void finalize();
public:
	virtual void setUp();
	virtual void tearDown();
};
#if defined(gForceAllTests) || defined(gMemoryTestFixture)
REGISTER_FIXTURE(MemoryTestFixture);
#endif