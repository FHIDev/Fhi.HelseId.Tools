#!/bin/bash

ClientId="37a08838-db82-4de0-bfe1-bed876e7086e"
# NewKey='{
#   "alg": "RSA",
#   "d": "Jc_CKOj_od8MSIzJK7BOYaFY1B5GDT76on9Z78Ncs3zJ0Z-ZOmqCFVNrgAZtN21EoNkPQ3Rt_jH6x7e1gAxVdDIsNJuValhq2ShzR1fB--3VdZiDhiGfhJViOZ0fF5vCjGnNJ7-dqG4WJt8g0zJJEGe4KUWS7_MKGeApPJE7CRRCJuTg6sFtYzxwRHGSYMMZSFkQWL2KHuYlsmdfg3xtOs3OZGwwGKr_K1ynaNmQqBxRE9avHH8i4Lhv4UNjDCsrhPRbGJRMr16XZZrpnkyl3exRqIO1gI8FOFBUrw5g5Jp19kT3GLgHUmjBa8bFbmKbNMcnVARSyhDFwoASCV_DLQ",
#   "dp": "GF07Ue928g0j7Puw5WBRu1-6TPPBVPm_-vuEPVv9WCfiT1i7fhHP9s2kAdgqg2JugTPOMmuoVlL1I_hI85ecaYknv_NLhDDmKDGtm1B9VKkISHSl5g5oQlpnASkN-Ojevqog2NZNp0twD2ifO5p-97H38f0S6dEwUrjs_05eUrc",
#   "dq": "C_xvog1GDv172tZsNZput7QwcTuyX_vNJFenvxy4Iema39FAvWFkpOpF7XaUABS67_92fzZ3QTuhPE7Y4wAeYueC3H4qXrkYT2DUUM4SqCOcuO27ZhopHQKbQU6wtvRwVzX1HsYdAkLzlMw0dk-Nl5KycABYklX72ihjZr2IwGU",
#   "e": "AQAB",
#   "key_ops": [],
#   "kid": "HZdw9CNQaVlLoRcQRf4tP7Nc9No1VsBE7fuzF6ruVSI",
#   "kty": "RSA",
#   "n": "0rN9JvdTq0Ex8jkcMsreXmswoyUSmaclJQZuQGolMj-hlZdGw62lJDsjAsNGljOs-9Xy6FUu9vdWFNHb57-EBwtZadj-ikJqNohQRYvAFe_hFCI960JnLe0Jcl7gFXsNLDdSa9mL-9CxB2gl1n41vFJ3FhZspleA_REywSAvPahAmITGqFN0U9WJFsHX5guh5x___lyAmDeHXhlxtlDk0f8GGLAOYc1tr10N6-qwXOUY8vFCtTrRyH4nydVWHYHWYLDEQst1v8w0UvZlEvYainiOE9B6HRsX_rszCKud_uhptQlvRj9PLD2EaXKlg0A_4zxaa2gzAP4OjTLCX2-jdQ",
#   "oth": [],
#   "p": "9w-iK5ooYhP2a1QCGhyQKydR5qbZVHOzBuluWUpL-lhFDK1JqrtHH97WvtkoG3KWbLPOVoCu_yPnt1UUUKsdolg5FRMIBT63cCEvc90CnsWIhASHktjCRyp55wxIzUr33Is_7ed2D87bocFIdK46UEmAiHUfhnr0jJYIKzql2cs",
#   "q": "2lMTrBtUZ0c19H5LtMSu5eGO1FLKnUFleJX964qwpA4AYsQhfGF6ZqocKCo5rc1FWj9b-YXL5XPd1-B-frg8jQNmB8HgMA9Mu-szxYq0mGx-r8Y_vTq28MD6uo2XYiCt3gLenHoUO8OCwcyTmmXhfD_k3sHa6GrhsyymtxDlz78",
#   "qi": "erCBFQiOiAHN7iKDO08g-oGUTnZTekAadvBl_tXQKXD5OXpF4SS9N42PQvtT4SgZJFplEWqQgYwG1IfQ-6yoO4AWZ69CsjUIclumAGgG3Xnfo_7qPpbUm6rI9qDCxyRTge-r5dwF-ABhJOjF0L1rmbl72NNc3epPIJDpkly1DAw",
#   "x5c": []
# }'
NewKey="{\"alg\":\"RSA\",\"d\":\"Jc_CKOj_od8MSIzJK7BOYaFY1B5GDT76on9Z78Ncs3zJ0Z-ZOmqCFVNrgAZtN21EoNkPQ3Rt_jH6x7e1gAxVdDIsNJuValhq2ShzR1fB--3VdZiDhiGfhJViOZ0fF5vCjGnNJ7-dqG4WJt8g0zJJEGe4KUWS7_MKGeApPJE7CRRCJuTg6sFtYzxwRHGSYMMZSFkQWL2KHuYlsmdfg3xtOs3OZGwwGKr_K1ynaNmQqBxRE9avHH8i4Lhv4UNjDCsrhPRbGJRMr16XZZrpnkyl3exRqIO1gI8FOFBUrw5g5Jp19kT3GLgHUmjBa8bFbmKbNMcnVARSyhDFwoASCV_DLQ\",\"dp\":\"GF07Ue928g0j7Puw5WBRu1-6TPPBVPm_-vuEPVv9WCfiT1i7fhHP9s2kAdgqg2JugTPOMmuoVlL1I_hI85ecaYknv_NLhDDmKDGtm1B9VKkISHSl5g5oQlpnASkN-Ojevqog2NZNp0twD2ifO5p-97H38f0S6dEwUrjs_05eUrc\",\"dq\":\"C_xvog1GDv172tZsNZput7QwcTuyX_vNJFenvxy4Iema39FAvWFkpOpF7XaUABS67_92fzZ3QTuhPE7Y4wAeYueC3H4qXrkYT2DUUM4SqCOcuO27ZhopHQKbQU6wtvRwVzX1HsYdAkLzlMw0dk-Nl5KycABYklX72ihjZr2IwGU\",\"e\":\"AQAB\",\"key_ops\":[],\"kid\":\"HZdw9CNQaVlLoRcQRf4tP7Nc9No1VsBE7fuzF6ruVSI\",\"kty\":\"RSA\",\"n\":\"0rN9JvdTq0Ex8jkcMsreXmswoyUSmaclJQZuQGolMj-hlZdGw62lJDsjAsNGljOs-9Xy6FUu9vdWFNHb57-EBwtZadj-ikJqNohQRYvAFe_hFCI960JnLe0Jcl7gFXsNLDdSa9mL-9CxB2gl1n41vFJ3FhZspleA_REywSAvPahAmITGqFN0U9WJFsHX5guh5x___lyAmDeHXhlxtlDk0f8GGLAOYc1tr10N6-qwXOUY8vFCtTrRyH4nydVWHYHWYLDEQst1v8w0UvZlEvYainiOE9B6HRsX_rszCKud_uhptQlvRj9PLD2EaXKlg0A_4zxaa2gzAP4OjTLCX2-jdQ\",\"oth\":[],\"p\":\"9w-iK5ooYhP2a1QCGhyQKydR5qbZVHOzBuluWUpL-lhFDK1JqrtHH97WvtkoG3KWbLPOVoCu_yPnt1UUUKsdolg5FRMIBT63cCEvc90CnsWIhASHktjCRyp55wxIzUr33Is_7ed2D87bocFIdK46UEmAiHUfhnr0jJYIKzql2cs\",\"q\":\"2lMTrBtUZ0c19H5LtMSu5eGO1FLKnUFleJX964qwpA4AYsQhfGF6ZqocKCo5rc1FWj9b-YXL5XPd1-B-frg8jQNmB8HgMA9Mu-szxYq0mGx-r8Y_vTq28MD6uo2XYiCt3gLenHoUO8OCwcyTmmXhfD_k3sHa6GrhsyymtxDlz78\",\"qi\":\"erCBFQiOiAHN7iKDO08g-oGUTnZTekAadvBl_tXQKXD5OXpF4SS9N42PQvtT4SgZJFplEWqQgYwG1IfQ-6yoO4AWZ69CsjUIclumAGgG3Xnfo_7qPpbUm6rI9qDCxyRTge-r5dwF-ABhJOjF0L1rmbl72NNc3epPIJDpkly1DAw\",\"x5c\":[]}"
OldKey="{\"alg\":\"PS512\",\"d\":\"BHnwAEpzq8XkCTMwR0hjhsrTz7DXLh5XPvh4q4rg0s-eA8unSn1-bvSuqeWb8MxSy9btJIrLiJ0A7Q-QC7uN-UP7L3s_5BunTz5CXx-rnGm0Q_E2MUtsUxVcsn-dcQSbqOWzA_vblGpdZJpbE6PspdukXR-OcdDzqy-J6YcreKVVrT3o-b5raI9bjcvUppJNK2CtEr-dYjyVWotML8e_GIf2wU2Gg_KBnX46JUp9DoedzMCthQX63yflYw-BMTFLJ-MvecrJQlQsrMjkNj8345C_vJU-gKIWFGAzhik7zwjxD68XUu7tV6-2Od0KjnFEQ7ARBZ9MiN3nnOIm42zLgvO0i0KSzqeW-QXbWKr1hEkWBenuXwwH3zzeU2WoLuXjxDIEQ7Mjava6U7AXBspbqgGg9wqyzeRCZapKXLx-nCnzTV5pM1RE5rWHJAiU6U2rPWEQGAiDT0art7E_4aa87QmL_fDXCAguPk7YniJUZ6_pln5ZpeSEza-ot-8KX8kVPJRmDDrSDoGE8aHJ39PtFEbuHQ_YQwtpg5Mj4px7l5nvKalox3z-JlgMGcx7dvEnENdukiIOYn0sPsbGv7g0irvDcIG7nBMUsm8memv4IT-ITTsyXt3IqCxgYF2VkFgy86TwFg0LA8NDGXhhdLa0aPWLNFGIvboBUEyF7aVshsE\"}"

# echo "NewKey: $NewKey"
# echo "OldKey: $OldKey"


export DOTNET_ENVIRONMENT=Development
 "C:\Users\jhoug\source\repos\Fhi.HelseId.Tools\src\ClientSecretTool\Fhi.HelseId.ClientSecret.App\bin\Debug\net9.0\Fhi.HelseId.ClientSecret.App.exe" updateclientkey --ClientId $ClientId --NewKey $NewKey --OldKey $OldKey  

