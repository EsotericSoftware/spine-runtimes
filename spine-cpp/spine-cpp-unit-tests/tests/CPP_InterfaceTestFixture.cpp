//////////////////////////////////////////////////////////////////////
//	filename: 	C_InterfaceTestFixture.cpp
//	
//	notes:		There is no C++ interface!
//
/////////////////////////////////////////////////////////////////////

#include "CPP_InterfaceTestFixture.h" 

CPP_InterfaceTestFixture::~CPP_InterfaceTestFixture()
{
	finalize();
}

void CPP_InterfaceTestFixture::initialize()
{
	// on a Per- Fixture Basis, before Test execution
}

void CPP_InterfaceTestFixture::finalize()
{
	// on a Per- Fixture Basis, after all tests pass/fail
}

void CPP_InterfaceTestFixture::setUp()
{
	// Setup on Per-Test Basis
}

void CPP_InterfaceTestFixture::tearDown()
{
	// Tear Down on Per-Test Basis
}

void CPP_InterfaceTestFixture::spineboyTestCase()
{
	// There is no C++ interface.
}

void CPP_InterfaceTestFixture::raptorTestCase()
{
	// There is no C++ interface.
}

void CPP_InterfaceTestFixture::goblinsTestCase()
{
	// No c++ interface
}
