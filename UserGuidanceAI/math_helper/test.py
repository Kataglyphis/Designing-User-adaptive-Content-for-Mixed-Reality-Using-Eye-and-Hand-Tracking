import unittest

# the files to best tested
import math_helper


class TestDummy(unittest.TestCase):

    def test_add(self) -> None:
        result = math_helper.clamp(10.234, -2.0, 100.0)
        self.assertEqual(result, 10.234)


if __name__ == "__main__":
    unittest.main()
