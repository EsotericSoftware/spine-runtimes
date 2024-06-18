#
# To learn more about a Podspec see http://guides.cocoapods.org/syntax/podspec.html.
# Run `pod lib lint spine_flutter.podspec` to validate before publishing.
#
Pod::Spec.new do |s|
  s.name                = 'SpineShadersStructs'
  s.version             = '0.0.1'
  s.summary             = 'Metal shaders structs for spine'
  s.description         = <<-DESC
Metal shaders structs for spine.
                       DESC
  s.homepage            = 'https://esotericsoftware.com'
  s.author              = { "Esoteric Software LLC  " => "https://esotericsoftware.com" }
  s.license             = { :file => 'LICENSE' }
  s.platform            = :ios, '13.0'

  s.source              = { :git => 'https://github.com/esotericsoftware/spine-runtimes.git', :branch => '4.2' }
  s.source_files        = 'spine-ios/Sources/SpineShadersStructs/*.{h,cpp}'

  s.pod_target_xcconfig = {
    'CLANG_CXX_LANGUAGE_STANDARD' => 'c++11',
    'CLANG_CXX_LIBRARY' => 'libc++'
  }
end
