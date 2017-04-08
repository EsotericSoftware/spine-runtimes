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

#pragma once

#include <string>

#include "teamcity_messages.h"

namespace JetBrains {

	class SourceLine
	{
	public:
		SourceLine():lineNumber(-1){}
		SourceLine(const std::string& theFile, int theLineNum):fileName(theFile),lineNumber(theLineNum){}
		~SourceLine(){}

		std::string fileName;
		int lineNumber;
		bool isValid() const {return (!fileName.empty() && lineNumber > -1);}
	};

	class TestFailure
	{
	public:
		std::string details;
		SourceLine sourceLine;
		std::string testName;
		std::string description;
	public:
		TestFailure(){}
		~TestFailure(){}

		TestFailure(const std::string& theTestName, const std::string& theDetails, SourceLine theSourcelLine, const std::string& theDescription)
		{
			testName = theTestName;
			details = theDetails;
			sourceLine = theSourcelLine;
			description = theDescription;
		}
	};

	class TeamcityProgressListener 
	{
	public:
		TeamcityMessages messages;
	public:
		TeamcityProgressListener(const std::string& _flowid);
		TeamcityProgressListener();
		~TeamcityProgressListener(){}

		void startTest(const std::string& test);
		void addFailure(const TestFailure &failure);
		void endTest(const std::string& test);
		void startSuite(const std::string& test);
		void endSuite(const std::string& test);
    
	private:
		std::string flowid;

		// Prevents the use of the copy constructor.
		TeamcityProgressListener(const TeamcityProgressListener &copy);

		// Prevents the use of the copy operator.
		void operator =(const TeamcityProgressListener &copy);
	};

}