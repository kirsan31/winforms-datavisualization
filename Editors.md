### Information about editors support in new WinForms designer.

- All not custom editors are fully working.
- **Almost all of the custom collection editors are working as defaults - without any custom changes.** This shows up in:  
  - There is no properties descriptions.  
  - Help doesn't work.  
  - There may be problems with complex actions, such as renaming dependent elements.  

  `AnnotationCollectionEditor` is the most problematic here - you can edit and delete annotations, but can't add them.  
  `AxesArrayEditor` is fully ported.
- Other custom editors.  
  Most of them are partially ported or working as defaults. But mostly these are cosmetic flaws and don't affect the work.  
  `ColorPaletteEditor` and `FlagsEnumUITypeEditor` are fully ported.