#
# To learn more about a Podspec see http://guides.cocoapods.org/syntax/podspec.html.
# Run `pod lib lint spine_flutter.podspec` to validate before publishing.
#
Pod::Spec.new do |s|
  s.name             = 'Spine'
  s.version          = '4.2.0'
  s.summary          = 'Spine runtimes for iOS.'
  s.description      = <<-DESC
Spine runtimes for iOS.
                       DESC
  s.homepage         = 'https://esotericsoftware.com'
  s.author           = { "Esoteric Software LLC  " => "https://esotericsoftware.com" }
  s.license          = { :file => 'LICENSE' }

  s.source           = { :git => 'https://github.com/esotericsoftware/spine-runtimes.git', :branch => '4.2' }
  s.source_files     = 'spine-ios/Sources/Spine/**/*.{swift,metal}'
  s.platform         = :ios, '13.0'

  s.xcconfig = {
    'HEADER_SEARCH_PATHS' => '"$(PODS_ROOT)/SpineCppLite/spine-cpp/spine-cpp/include" "$(PODS_ROOT)/SpineCppLite/spine-cpp/spine-cpp-lite"'
  }

  s.swift_version = '5.0'
  s.dependency 'SpineCppLite'
  s.dependency 'SpineShadersStructs'
end
