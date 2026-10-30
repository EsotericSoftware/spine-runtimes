#
# To learn more about a Podspec see http://guides.cocoapods.org/syntax/podspec.html.
# Run `pod lib lint spine_flutter.podspec` to validate before publishing.
#
Pod::Spec.new do |s|
  s.name             = 'spine_flutter'
  s.version          = '0.0.1'
  s.summary          = 'A new Flutter FFI plugin project.'
  s.description      = <<-DESC
A new Flutter FFI plugin project.
                       DESC
  s.homepage         = 'http://example.com'
  s.license          = { :file => '../LICENSE' }
  s.author           = { 'Your Company' => 'email@example.com' }
  s.source           = { :path => '.' }
  s.source_files     = 'spine_flutter/Sources/spine_flutter/spine-c/src/**/*.{cpp,h}', 'spine_flutter/Sources/spine_flutter/spine-cpp/src/**/*.{cpp,h}'
  s.xcconfig = { 'HEADER_SEARCH_PATHS' => '"$(PODS_TARGET_SRCROOT)/spine_flutter/Sources/spine_flutter/spine-cpp/include" "$(PODS_TARGET_SRCROOT)/spine_flutter/Sources/spine_flutter/spine-c/include"' }
  s.dependency 'FlutterMacOS'

  s.platform = :osx, '10.11'
  s.pod_target_xcconfig = { 'DEFINES_MODULE' => 'YES' }
  s.swift_version = '5.0'
end
