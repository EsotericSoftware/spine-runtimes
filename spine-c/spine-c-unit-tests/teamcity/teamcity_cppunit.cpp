/* Copyright 2011 JetBrains s.r.o.
 * 
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at
 * 
 *     http://www.apache.org/licenses/LICENSE-2.0
 * 
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 * 
 * $Revision: 88625 $
*/

#include <sstream>

#include "teamcity_cppunit.h"

using namespace std;

namespace JetBrains {

TeamcityProgressListener::TeamcityProgressListener()
{
    flowid = getFlowIdFromEnvironment();
}

TeamcityProgressListener::TeamcityProgressListener(const std::string& _flowid)
{
    flowid = _flowid;
}

void TeamcityProgressListener::startTest(const std::string& test) {
    messages.testStarted(test, flowid);
}

static string sourceLine2string(const SourceLine &sline) {
    stringstream ss;
        
    ss << sline.fileName << ":" << sline.lineNumber;
    
    return ss.str();
}

void TeamcityProgressListener::addFailure(const TestFailure &failure) 
{
  
    string details = failure.details;
    
    if (failure.sourceLine.isValid()) {
        details.append(" at ");
        details.append(sourceLine2string(failure.sourceLine));
        details.append("\n");
    }
    
    messages.testFailed(
        failure.testName,
        failure.description,
        details,
        flowid
    );
}

void TeamcityProgressListener::endTest(const std::string& test) 
{
    messages.testFinished(test, -1, flowid);
}

void TeamcityProgressListener::startSuite(const std::string& test) 
{
    messages.suiteStarted(test, flowid);
}

void TeamcityProgressListener::endSuite(const std::string& test) 
{
    messages.suiteFinished(test, flowid);
}

}
