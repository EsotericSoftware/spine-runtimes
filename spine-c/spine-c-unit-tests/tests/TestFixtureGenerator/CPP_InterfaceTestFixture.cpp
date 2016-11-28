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
