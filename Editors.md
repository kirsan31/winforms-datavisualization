### Information about editors support in new WinForms designer.

- All non custom editors are fully ported.
- All non collection custom editors are fully ported.
- **Almost all of the custom collection editors are partially ported.**  
  This shows up in that there may be problems with complex actions, such as renaming dependent elements.  
  `AnnotationCollectionEditor` is the most problematic here - you can edit and delete annotations, but can't add them.  
  `AxesArrayEditor` is fully ported.