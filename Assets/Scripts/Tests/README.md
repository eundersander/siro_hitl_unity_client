Tests can be run from within the Unity Editor. See instructions [here](https://docs.unity3d.com/2017.4/Documentation/Manual/testing-editortestsrunner.html).

Unity tests are split into two types: Play Mode and Edit Mode. See [the documentation](https://docs.unity3d.com/Packages/com.unity.test-framework@1.0/manual/edit-mode-vs-play-mode-tests.html) for reference.

**Play mode** tests are run in-engine, allowing to test how the application behaves once launched. They are slow. Use for the following:
* Integration tests.
* Features that rely on MonoBehavior.

**Edit mode** tests are run from the Editor, therefore are much faster. Use for testing the following:
* "Classic" unit tests that don't use MonoBehavior.
* Editor tools.