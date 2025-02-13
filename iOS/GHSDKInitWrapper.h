//
//  GHSDKInitWrapper.h
//  MeteorShowSDK
//
//  Created by steve on 2024/12/20.
//  Copyright © 2024 . All rights reserved.
//

#import <Foundation/Foundation.h>

@interface GHSDKInitWrapper : NSObject

+ (void)initSDK:(void(^)(void))handler;

@end
