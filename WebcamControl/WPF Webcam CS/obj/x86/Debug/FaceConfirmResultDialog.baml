   M S B A M L   `   `   `  ����H  DWPF Webcam CS, Version=3.2.0.0, Culture=neutral, PublicKeyToken=null-clr-namespace:WPF_Webcam_CSWPF_Webcam_CS  5   	   +    %WPF_Webcam_CS.FaceConfirmResultDialog   5   	   R NWindowsBase, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35\ XPresentationFramework, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35W SPresentationCore, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35D 9http://schemas.microsoft.com/winfx/2006/xaml/presentation    5   	   R NSystem.Xaml, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089:x,http://schemas.microsoft.com/winfx/2006/xaml     5   	   @d2http://schemas.microsoft.com/expression/blend/2008     5   	   Jmc;http://schemas.openxmlformats.org/markup-compatibility/2006     5   	   -localclr-namespace:WPF_Webcam_CS     5   	     � WindowStartupLocation$  CenterScreen=�5   9    � Title$ FaceConfirmResultDialog��5   	   $	��300��6)   $	��600��66    � WindowStyle$
 None=�6B   .��5	      �� $��5q�6   ���#5F9EA0 �� CornerRadius$ 20��67   
�������._�5
   
   �  � RowDefinitions 5      �� 5      $
��0.5* �6    60   �� 5      $��8* �6    6.   �� 5      $��1* �6    6.    � ColumnDefinitions 5      �� 5      $��7* �6#   60   �� 5      $
��0.3* �6#   62   �� 5      $��4* �6#   60   .+�5      �� $��0��6   $��0��6"   ���#5F9EA062   �� 5      $��0��6   $��1��6"   ���#5F9EA062   �� 5      $��0��6   $��2��6"   ���#5F9EA062   �� 5      -   5        $��	userImage��5      $��1��6%   $#��/Image/successful-icon-20.jpg��62   5      (� 5      $��1��6   $��2��6$   .��5      ��  �� Orientation$ Vertical=�6   .+�5      �� $ 
Horizontal=�6!   .+�5      �� $#��/Image/successful-icon-20.jpg��6    $	��100��6G   6V   �� 5      -   5        $��userPercentage��5   $   $	��90%��6:   $��90(�6E   
���  ��$��DemiBold"�6x   6�   5      �� 5       $��30��6    6.   �� 5!      $ 
Horizontal=�6!   .+�5"      �� $��40(�6$   $'��!あなたのアカウント名は��62   6G   �� 5#      -   5         ��Name$ 
txtMessage��5#   $   $��40(�68   
���  ��$��DemiBold"�6a   $
��Wrap=�6�   $��Center=�6�   $��Center=�6�    �� TextAlignment$
 Left=�6�   $"��よろしいでしょうか?��6�   �� 5$      $��40(�6$   $��ですか？��62   6@   5%      �� 5&      $��30��6    6.   �� 5'      $��40(�6    $$��よろしいでしょうか？��6.   6B   5(      5)      (� 5+      $��2��6   $��2��6#   .��5,      �� $ 
Horizontal=�6   .+�5-      �� -   5        	 ��Name$	 No��5-   S   
����������#5F9EA0$��No��6F   $��1q�6_   $
��Left=�6j   $	��Top=�6�   $��55��6�   6�   �� 5.      -   5        $		 Yes��5.   r   ���#5F9EA0���#5F9EA0
�������$��OK��6e   $��1q�6   $
��Left=�6�   $	��Top=�6�   $��55��6�   6�   5/      50      51      52      53      