import SwiftUI

struct SimpleAnimationViewControllerRepresentable: UIViewControllerRepresentable {
    typealias UIViewControllerType = SimpleAnimationViewController
    
    func makeUIViewController(context: Context) -> SimpleAnimationViewController {
        return SimpleAnimationViewController()
    }
    
    func updateUIViewController(_ uiViewController: SimpleAnimationViewController, context: Context) {
        //
    }
}
