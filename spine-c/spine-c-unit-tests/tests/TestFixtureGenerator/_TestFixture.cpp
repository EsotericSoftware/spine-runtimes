#include "[[FIXTURE_TYPE]].h" 

[[FIXTURE_TYPE]]::~[[FIXTURE_TYPE]]()
{
	finalize();
}

void [[FIXTURE_TYPE]]::initialize()
{
	// on a Per- Fixture Basis, before Test execution
}

void [[FIXTURE_TYPE]]::finalize()
{
	// on a Per- Fixture Basis, after all tests pass/fail
}

void [[FIXTURE_TYPE]]::setUp()
{
	// Setup on Per-Test Basis
}

void [[FIXTURE_TYPE]]::tearDown()
{
	// Tear Down on Per-Test Basis
}
