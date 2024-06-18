#
# To learn more about a Podspec see http://guides.cocoapods.org/syntax/podspec.html.
# Run `pod lib lint spine_flutter.podspec` to validate before publishing.
#
Pod::Spec.new do |s|
  s.name                = 'SpineCppLite'
  s.version             = '0.0.1'
  s.summary             = 'Spine runtimes for iOS.'
  s.description         = <<-DESC
Spine runtimes for iOS.
                       DESC
  s.homepage            = 'https://esotericsoftware.com'
  s.author              = { "Esoteric Software LLC  " => "https://esotericsoftware.com" }
  s.license             = { :file => 'LICENSE' }
  s.platform            = :ios, '13.0'

  s.source              = { :git => 'https://github.com/esotericsoftware/spine-runtimes.git', :branch => 'cocoapods' }
  s.source_files        =  'spine-cpp/spine-cpp/**/*.{h,cpp}', 'spine-cpp/spine-cpp-lite/*.{h,cpp}'
  s.module_map          = 'spine-cpp/spine-cpp-lite/module.modulemap'
  s.pod_target_xcconfig = {
    'HEADER_SEARCH_PATHS' => '"$(PODS_ROOT)/SpineCppLite/spine-cpp/spine-cpp/include" "$(PODS_ROOT)/SpineCppLite/spine-cpp/spine-cpp-lite"',
    'CLANG_CXX_LANGUAGE_STANDARD' => 'c++11',
    'CLANG_CXX_LIBRARY' => 'libc++'
  }
end
