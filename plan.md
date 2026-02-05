# خطة عمل برنامج سيرفر طباعة (Windows)

## 1) أهداف البرنامج
- تمكين الطباعة من نفس الجهاز ومن أجهزة الشبكة بسهولة.
- دعم الأنواع الشائعة: نصوص، صور، PDF.
- دعم الطباعة الخام RAW/ZPL لطابعات الملصقات.
- واجهة تحكم واضحة لا تتطلب خبرة تقنية.
- سجل طباعة مفصل للتتبع والمراجعة.
- أمان بسيط عبر توكن قابل للتوليد من الواجهة.
- استقرار واعتمادية مع Queue للطباعة.

---

## 2) نطاق العمل (Scope)
**يشمل:**
- خدمة Windows تعمل كـ Print Server.
- Web UI لإدارة الإعدادات وسجل الطباعة.
- REST API للطباعة من الشبكة.
- دعم طباعة عبر Windows Driver + RAW/ZPL + TCP Direct.
- قاعدة بيانات خفيفة محلية لحفظ الإعدادات والسجل.

**لا يشمل (مستقبلي):**
- تكامل مع أنظمة سحابية خارجية.
- Multi-tenant أو صلاحيات متقدمة لكل مستخدم.
- Billing أو إدارة فواتير.

---

## 3) المتطلبات الوظيفية
- اختيار الطابعة الافتراضية من قائمة طابعات Windows.
- إرسال أمر طباعة عبر API أو واجهة ويب.
- دعم أنواع:
  - PDF
  - Images (PNG/JPG)
  - Text
  - RAW
  - ZPL
- عرض سجل الطباعة:
  - وقت التنفيذ
  - نوع الملف
  - الطابعة
  - الحالة (نجاح/فشل)
  - سبب الخطأ
- توليد/تعطيل Token من الواجهة.
- دعم طباعة RAW/ZPL:
  - عبر Winspool RAW
  - عبر TCP Direct (Port 9100)

---

## 4) المتطلبات غير الوظيفية
- عدد الطابعات: حتى 10
- عدد المستخدمين: حتى 100
- الأداء: طباعة متزامنة عبر Queue
- الاستقرار: Retry بسيط للأخطاء المؤقتة
- الأمان: توكن + حد حجم الملفات
- التشغيل: Windows Service

---

## 5) التقنية المقترحة
- Backend: .NET 8 (ASP.NET Core)
- قاعدة بيانات: SQLite محلية (Settings + History)
- طباعة PDF/Images: عبر Windows Print Spooler
- تحويل النص إلى PDF قبل الطباعة
- RAW/ZPL عبر Winspool أو TCP Socket

---

## 6) التصميم المعماري (High-level)
- **Service Layer**
  - Worker/Queue
  - Printer Adapters (Driver / RAW / TCP)
- **API Layer**
  - REST endpoints للطباعة والإعدادات
- **UI Layer**
  - لوحة تحكم (إعدادات + سجل)
- **Data Layer**
  - Settings
  - Print History

---

## 7) مواصفات API (تفصيلية)

### المصادقة
- جميع الطلبات الخارجية تتطلب Header:
  - `X-Print-Token: <token>`
- الطلبات من نفس الجهاز يمكن السماح بها بدون توكن (اختياري عبر إعداد).

### 7.1 GET /printers
**الوصف:** إرجاع قائمة طابعات Windows المتاحة  
**الاستجابة:**
```json
{
  "printers": [
    {
      "name": "HP LaserJet P1102",
      "isDefault": true,
      "supportsRaw": true
    },
    {
      "name": "Zebra ZD220",
      "isDefault": false,
      "supportsRaw": true
    }
  ]
}
```

### 7.2 POST /print
**الوصف:** إرسال مهمة طباعة  
**المحتوى:** `multipart/form-data` أو `application/json`

**حالة (ملف)**
- `type`: pdf | image | raw | zpl
- `file`: الملف المرسل
- `printer`: اختياري (اسم الطابعة)
- `rawMode`: winspool | tcp (اختياري)

**حالة (نص)**
- `type`: text
- `text`: النص المراد طباعته
- `printer`: اختياري

**الاستجابة:**
```json
{
  "jobId": "8f4e1a2c",
  "status": "queued",
  "printer": "HP LaserJet P1102"
}
```

### 7.3 GET /history
**الوصف:** سجل آخر المهام  
**المعاملات (Query):**
- `status`: success | failed | queued
- `printer`: اسم الطابعة
- `from`: تاريخ البداية
- `to`: تاريخ النهاية
- `limit`: عدد النتائج (افتراضي 100)

**الاستجابة:**
```json
{
  "items": [
    {
      "jobId": "8f4e1a2c",
      "time": "2026-02-05T13:20:11Z",
      "type": "pdf",
      "printer": "HP LaserJet P1102",
      "status": "success",
      "size": 153421,
      "clientIP": "192.168.1.20",
      "error": null
    }
  ]
}
```

### 7.4 GET /settings
**الوصف:** قراءة الإعدادات  
**الاستجابة:**
```json
{
  "defaultPrinter": "HP LaserJet P1102",
  "rawMode": "tcp",
  "rawTcpHost": "192.168.1.50",
  "rawTcpPort": 9100,
  "rawEncoding": "utf-8",
  "rawTerminator": "^XZ",
  "allowLocalNoToken": true,
  "maxFileSizeMB": 20
}
```

### 7.5 PUT /settings
**الوصف:** تعديل الإعدادات  
**الطلب:**
```json
{
  "defaultPrinter": "Zebra ZD220",
  "rawMode": "tcp",
  "rawTcpHost": "192.168.1.50",
  "rawTcpPort": 9100,
  "rawEncoding": "ansi",
  "rawTerminator": "^XZ",
  "allowLocalNoToken": true,
  "maxFileSizeMB": 20
}
```

### 7.6 POST /token/generate
**الوصف:** توليد توكن جديد  
**الاستجابة:**
```json
{
  "token": "abc123xyz"
}
```

### 7.7 POST /token/disable
**الوصف:** تعطيل التوكن الحالي  
**الاستجابة:**
```json
{
  "disabled": true
}
```

---

## 8) واجهة التحكم (تفصيلية)
**الصفحات:**
1) لوحة الإعدادات
   - اختيار الطابعة الافتراضية
   - اختيار طريقة RAW (Winspool / TCP)
   - ضبط IP/Port (افتراضي 9100)
   - Encoding (UTF-8 / ANSI)
   - Terminator (مثل ^XZ)

2) لوحة التوكن
   - توليد / تعطيل
   - عرض التوكن الحالي

3) سجل الطباعة
   - قائمة آخر الطلبات
   - فلترة حسب الحالة/الطابعة/التاريخ
   - عرض تفاصيل الخطأ

---

## 9) منطق الطباعة
- **Driver Path**
  - PDF/Images -> إرسال للـ Print Spooler
  - Text -> تحويل PDF ثم طباعة
- **RAW/ZPL Path**
  - إذا الطابعة متاحة ضمن Windows: Winspool RAW
  - إذا طابعة شبكة مباشرة: TCP Direct إلى 9100
- **Auto-Select**
  - حسب إعدادات المستخدم (RawMode)

---

## 10) خطة التنفيذ المرحلية

### Phase 0 — التحضير
- تثبيت .NET 8 SDK
- تحديد منفذ الخدمة (مثلاً 5050)
- تحديد حجم ملفات مسموح (مثلاً 20MB)

### Phase 1 — الهيكل الأساسي
- مشروع ASP.NET Core + Worker Service
- تفعيل Windows Service

### Phase 2 — قاعدة البيانات
- Settings table
- History table

### Phase 3 — طبقة الطباعة
- Queue داخلية
- Driver Path
- RAW/ZPL Path
- TCP Direct Path

### Phase 4 — API
- print / printers / history / settings / token

### Phase 5 — واجهة التحكم
- إعدادات
- توكن
- سجل

### Phase 6 — الأمان
- توكن + حجم ملفات
- تسجيل IP

### Phase 7 — النشر
- Build Release
- Setup Windows Service
- فتح منفذ في Firewall

### Phase 8 — الاختبار
- طباعة محلية
- طباعة من الشبكة
- PDF/Images/Text/ZPL
- اختبار الاستقرار والضغط

---

## 11) متطلبات التنصيب السلس (Easy Install)

### الهدف
المستخدم النهائي غير مبرمج، لذلك التنصيب يجب أن يكون:
- بنقرة واحدة
- بدون تثبيت إضافي يدوي
- بدون إعدادات معقدة

### منهجية التنصيب المقترحة
1) نسخة Installer واحدة (MSI أو EXE)
   - تثبت الخدمة كـ Windows Service تلقائيا
   - تفتح المنفذ في Firewall تلقائيا (اختياري عبر checkbox)

2) تضمين كل المتطلبات داخل الحزمة
   - نشر Self-Contained (.NET Runtime مدمج)
   - لا حاجة لتثبيت .NET يدويا

3) إعدادات أولية تلقائية
   - اكتشاف الطابعات وتعيين الافتراضية تلقائيا
   - إنشاء توكن افتراضي تلقائيا
   - منفذ افتراضي جاهز (5050)

4) واجهة تشغيل واضحة
   - اختصار على سطح المكتب لفتح لوحة التحكم
   - صفحة ترحيب تعرض:
     - رابط الواجهة
     - التوكن الحالي
     - حالة الخدمة

5) التحديث
   - خيار "Check for Updates" (اختياري لاحقا)
   - إمكانية استبدال النسخة دون فقد السجل

---

## 12) المخاطر المحتملة
- اختلاف Drivers للطابعات
- مشاكل Encoding في ZPL
- انقطاع الشبكة أثناء TCP Direct
- بطء الطباعة مع ملفات كبيرة

**حلول:**
- Queue + Retry
- خيارات Encoding
- Logging مفصل

---

## 13) المخرجات النهائية
- برنامج يعمل كخدمة Windows
- لوحة تحكم مفصلة
- API جاهز للاستخدام من أي جهاز بالشبكة
- سجل طباعة واضح

---

## 14) الخطوة التالية
- تحويل هذه الخطة إلى Timeline يومي/أسبوعي (اختياري)
- إعداد خطة MVP خلال 7-10 أيام
