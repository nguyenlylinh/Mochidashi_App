   M S B A M L   `   `   `  ����H  DWPF Webcam CS, Version=3.2.0.0, Culture=neutral, PublicKeyToken=null-clr-namespace:WPF_Webcam_CSWPF_Webcam_CS  5   	   !    WPF_Webcam_CS.SuccessDialog   5   	   R NWindowsBase, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35\ XPresentationFramework, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35W SPresentationCore, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35D 9http://schemas.microsoft.com/winfx/2006/xaml/presentation    5   	   R NSystem.Xaml, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089:x,http://schemas.microsoft.com/winfx/2006/xaml     5   	   @d2http://schemas.microsoft.com/expression/blend/2008     5   	   Jmc;http://schemas.openxmlformats.org/markup-compatibility/2006     5   	   -localclr-namespace:WPF_Webcam_CS     5   	     � WindowStartupLocation$  CenterScreen=�5   /    � Title$ SuccessDialog��5   	   $	��300��6   $	��600��6,    � WindowStyle$
 None=�69   .��5	      �� $��5q�6   ���#5F9EA0 �� CornerRadius$ 20��67   
�������._�5
   
   �  � RowDefinitions 5      �� 5      $��1* �6    6.   �� 5      $��3* �6    6.   �� 5      $��1* �6    6.    � ColumnDefinitions 5      �� 5      $
��1.5* �6#   62   �� 5      $
��0.3* �6#   62   �� 5      $	��10* �6#   61   .+�5      �� $��0��6   $��0��6"   ���#5F9EA062   �� 5      $��0��6   $��1��6"   ���#5F9EA062   �� 5      $��0��6   $��2��6"   ���#5F9EA062   �� 5      $��1��6   $#��/Image/successful-icon-20.jpg��6"   5      (� 5      $��1��6   $��2��6$   .��5      �� -   5         ��Name$ 
txtMessage��5      $	��420��60   $��25(�6<   $
��Wrap=�6J   $��Center=�6^   $��Center=�6{    �� TextAlignment$
 Left=�6�   $"��よろしいでしょうか?��6�   5      (� 5      $��2��6   $��2��6#   .��5      ��  �� Orientation$ 
Horizontal=�6   .+�5       �� -   5        	 ��Name$		 Yes��5    S   
����������#5F9EA0$��OK��6F   $��1q�6`   $
��Left=�6k   $	��Top=�6�   $��55��6�   6�   5!      5"      5#      5$      5&      