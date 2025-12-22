namespace Tsumiki.View;

internal partial class TuningValueControl(
    RectF control,
    IRangeViewParameter<int> n,
    IRangeViewParameter<int> d,
    IRangeViewParameter<int> pn,
    IRangeViewParameter<int> pd) : Control(control)
{
    private static readonly string ValidChars = "0123456789^/cv";
    private static readonly Dictionary<VirtualKeyCode, char> ValidKeyCodes = new()
    {
        [VirtualKeyCode.Divide] = '/',
        [VirtualKeyCode.NumPad0] = '0',
        [VirtualKeyCode.NumPad1] = '1',
        [VirtualKeyCode.NumPad2] = '2',
        [VirtualKeyCode.NumPad3] = '3',
        [VirtualKeyCode.NumPad4] = '4',
        [VirtualKeyCode.NumPad5] = '5',
        [VirtualKeyCode.NumPad6] = '6',
        [VirtualKeyCode.NumPad7] = '7',
        [VirtualKeyCode.NumPad8] = '8',
        [VirtualKeyCode.NumPad9] = '9',
        [VirtualKeyCode.Back] = '\b',
    };
    private static readonly RectF[] Textures = CreateDigitRects(PixelToTexture(2080, 60, 25, 30));
    private static readonly SizeF TextureSize = PixelToControlSize(25, 30);
    private static readonly RectF BarTexture = PixelToTexture(2410, 15, 75, 30);
    private static readonly RectF HatTexture = PixelToTexture(2485, 0, 15, 30);
    private static readonly SizeF BarSize = PixelToControlSize(75, 30);
    private static readonly SizeF HatSize = PixelToControlSize(15, 30);
    private static int CopiedN = 1;
    private static int CopiedD = 1;
    private static int CopiedPn = 1;
    private static int CopiedPd = 1;
    private readonly SizeF _halfSize = control.Size * 0.5f;
    private readonly SizeF _size = control.Size;
    private readonly byte[] _nDigits = new byte[3];
    private readonly byte[] _dDigits = new byte[3];
    private readonly byte[] _pnDigits = new byte[3];
    private readonly byte[] _pdDigits = new byte[3];
    private bool _isFirstRender = true;
    private int _nLength = 0;
    private int _dLength = 0;
    private int _pnLength = 0;
    private int _pdLength = 0;
    private FocusPart _focusedPart = FocusPart.N;

    private static RectF[] CreateDigitRects(in RectF baseRect)
    {
        var result = new RectF[11];
        for (var i = 0; i < 11; i++)
        {
            result[i] = new RectF(
                baseRect.Left + baseRect.Width * i,
                baseRect.Top,
                baseRect.Left + baseRect.Width * (i + 1),
                baseRect.Bottom);
        }

        return result;
    }

    internal override void OnLeftButtonUp(PointF point)
    {
        if (_size.Contains(point) && _draggingParameter is null)
        {
            Focus();
            RequestRender();
        }

        _draggingParameter?.EndEdit();
        _draggingParameter = null;
    }

    internal override void OnLostFocus()
    {
        if (_focusedPart == FocusPart.N && _nLength == 0)
        {
            n.NormalizedValue = n.DefaultNormalizedValue;
            d.NormalizedValue = d.DefaultNormalizedValue;
            pn.NormalizedValue = pn.DefaultNormalizedValue;
            pd.NormalizedValue = pd.DefaultNormalizedValue;
        }
        else
        {
            SetParameter(FocusPart.N);
            SetParameter(FocusPart.D);
            SetParameter(FocusPart.Pn);
            SetParameter(FocusPart.Pd);

            int GetValue(FocusPart focusPart)
            {
                var digits = FocusedPartDigits(focusPart);
                var length = FocusedPartLength(focusPart);

                // 未入力の場合は1を返す。全体として (1/1)^(1/1) がデフォルト値となる
                if (length == 0) return 1;

                var value = 0;
                for (var i = 0; i < length; i++)
                {
                    value = value * 10 + digits[i];
                }

                return value;
            }

            void SetParameter(FocusPart focusPart)
            {
                var parameter = FocusedParameter(focusPart);
                var value = Math.Clamp(GetValue(focusPart), parameter.MinValue, parameter.MaxValue);
                SetParameterValue(parameter, value);
            }

            void SetParameterValue(IRangeViewParameter<int> parameter, int value)
            {
                if (parameter.Value != value)
                {
                    parameter.BeginEdit();
                    parameter.Value = value;
                    parameter.EndEdit();
                }
            }
        }

        LoadParameters();
        RequestRender();
        base.OnLostFocus();
    }

    /// <summary>フォーカスをひとつ前のパートに移動する</summary>
    private void FocusBackPart()
    {
        ref var length = ref FocusedPartLength(_focusedPart);
        if (length > 0)
        {
            length--;
        }
        else
        {
            while (FocusPart.N < _focusedPart)
            {
                _focusedPart--;
                if (0 < FocusedPartLength(_focusedPart)) return;
            }
        }
    }

    private byte[] FocusedPartDigits(FocusPart focusPart) =>
        focusPart switch
        {
            FocusPart.N => _nDigits,
            FocusPart.D => _dDigits,
            FocusPart.Pn => _pnDigits,
            FocusPart.Pd => _pdDigits,
            _ => throw new InvalidOperationException(),
        };

    private ref int FocusedPartLength(FocusPart focusPart)
    {
        switch (focusPart)
        {
            case FocusPart.N: return ref _nLength;
            case FocusPart.D: return ref _dLength;
            case FocusPart.Pn: return ref _pnLength;
            case FocusPart.Pd: return ref _pdLength;
            default: throw new InvalidOperationException();
        }
    }

    private IRangeViewParameter<int> FocusedParameter(FocusPart focusPart)
    {
        return focusPart switch
        {
            FocusPart.N => n,
            FocusPart.D => d,
            FocusPart.Pn => pn,
            FocusPart.Pd => pd,
            _ => throw new InvalidOperationException(),
        };
    }

    private void LoadParameters()
    {
        LoadParameters(n.Value, d.Value, pn.Value, pd.Value);
    }

    /// <summary>パラメータから表示用データに値を読み込む</summary>
    private void LoadParameters(int nValue, int dValue, int pnValue, int pdValue)
    {
        LoadValue(FocusPart.N, nValue);
        LoadOrClearValue(FocusPart.D, dValue);

        if (pnValue == 1 && pdValue == 1)
        {
            ClearValue(FocusPart.Pn);
            ClearValue(FocusPart.Pd);
        }
        else
        {
            LoadValue(FocusPart.Pn, pnValue);
            LoadOrClearValue(FocusPart.Pd, pdValue);
        }

        void LoadOrClearValue(FocusPart focusPart, int value)
        {
            if (value == 1)
            {
                ClearValue(focusPart);
            }
            else
            {
                LoadValue(focusPart, value);
            }
        }

        void LoadValue(FocusPart focusPart, int value)
        {
            _focusedPart = focusPart;
            var digits = FocusedPartDigits(focusPart);
            ref var length = ref FocusedPartLength(focusPart);

            if (value == 0)
            {
                length = 1;
                digits[0] = 0;
                return;
            }
        
            length = 0;
            if (value >= 100)
            {
                digits[length] = (byte)(value / 100);
                length++;
            }
        
            if (value >= 10)
            {
                digits[length] = (byte)(value / 10 % 10);
                length++;
            }

            if (value >= 1)
            {
                digits[length] = (byte)(value % 10);
                length++;
            }
        }

        void ClearValue(FocusPart focusPart)
        {
            FocusedPartLength(focusPart) = 0;
        }
    }

    internal override bool OnKeyDown(char key, VirtualKeyCode keyCode, KeyModifier modifiers)
    {
        if (keyCode == VirtualKeyCode.Back)
        {
            FocusBackPart();
        }
        else if (key == 'c')
        {
            CopiedN = n.Value;
            CopiedD = d.Value;
            CopiedPn = pn.Value;
            CopiedPd = pd.Value;
            return true;
        }
        else if (key == 'v')
        {
            LoadParameters(CopiedN, CopiedD, CopiedPn, CopiedPd);
            RequestRender();
            return true;
        }
        else
        {
            char targetChar;
            if (ValidChars.Contains(key))
            {
                targetChar = key;
            }
            else if (!ValidKeyCodes.TryGetValue(keyCode, out targetChar))
            {
                return false;
            }

            switch (targetChar)
            {
                case '/':
                    if (_focusedPart is FocusPart.N or FocusPart.Pn)
                    {
                        // 未入力の分子がある場合は1を補完
                        if (FocusedPartLength(_focusedPart) == 0) AddDigit(_focusedPart, 1);
                        _focusedPart++;
                    }
                    break;

                case '^':
                    if (_focusedPart is FocusPart.N or FocusPart.D)
                    {
                        // 未入力の分子がある場合は1を補完。分母が未入力の場合は1が補完される
                        if (FocusedPartLength(FocusPart.N) == 0) AddDigit(FocusPart.N, 1);
                        _focusedPart = FocusPart.Pn;
                    }
                    break;

                default:
                    AddDigit(_focusedPart, (byte)(targetChar - '0'));
                    break;
            }
        }

        RequestRender();
        return true;

        void AddDigit(FocusPart focusPart, byte value)
        {
            ref var length = ref FocusedPartLength(focusPart);
            var digits = FocusedPartDigits(focusPart);
            if (length < digits.Length)
            {
                digits[length] = value;
                length++;
            }
        }
    }

    internal override bool OnKeyUp(char key, VirtualKeyCode keyCode, KeyModifier modifiers)
    {
        return ValidChars.Contains(key) || ValidKeyCodes.TryGetValue(keyCode, out _);
    }

    public override void OnParameterChanged(int parameterId)
    {
        if (parameterId == n.Id || parameterId == d.Id || parameterId == pn.Id || parameterId == pd.Id)
        {
            if (!IsFocused)
                LoadParameters();
            RequestRender();
        }
    }

    internal override void RenderCore(IDrawingContext context)
    {
        if (_isFirstRender)
        {
            _isFirstRender = false;
            LoadParameters();
        }

        switch (_focusedPart)
        {
            case FocusPart.N:
                RenderDigits(context, FocusPart.N, new(GlobalRect.Left + _halfSize.Width, GlobalRect.Top + _halfSize.Height - TextureSize.Height * 0.5f));
                break;

            case FocusPart.D:
                RenderBar(context, _halfSize.Width - BarSize.Width * 0.5f);
                RenderDigits(context, FocusPart.N, new(GlobalRect.Left + _halfSize.Width, GlobalRect.Top));
                RenderDigits(context, FocusPart.D, new(GlobalRect.Left + _halfSize.Width, GlobalRect.Bottom - TextureSize.Height));
                break;

            case FocusPart.Pn:
                RenderHat(context);
                if (_dLength == 0)
                {
                    RenderDigits(context, FocusPart.N, new(GlobalRect.Left + TextureSize.Width * 1.5f, GlobalRect.Top + _halfSize.Height - TextureSize.Height * 0.5f));
                }
                else
                {
                    RenderBar(context, 0f);
                    RenderDigits(context, FocusPart.N, new(GlobalRect.Left + TextureSize.Width * 1.5f, GlobalRect.Top));
                    RenderDigits(context, FocusPart.D, new(GlobalRect.Left + TextureSize.Width * 1.5f, GlobalRect.Bottom - TextureSize.Height));
                }
                RenderDigits(context, FocusPart.Pn, new(GlobalRect.Right - TextureSize.Width * 1.5f, GlobalRect.Top + _halfSize.Height - TextureSize.Height * 0.5f));
                break;

            case FocusPart.Pd:
                RenderHat(context);
                if (_dLength == 0)
                {
                    RenderDigits(context, FocusPart.N, new(GlobalRect.Left + TextureSize.Width * 1.5f, GlobalRect.Top + _halfSize.Height - TextureSize.Height * 0.5f));
                }
                else
                {
                    RenderBar(context, 0f);
                    RenderDigits(context, FocusPart.N, new(GlobalRect.Left + TextureSize.Width * 1.5f, GlobalRect.Top));
                    RenderDigits(context, FocusPart.D, new(GlobalRect.Left + TextureSize.Width * 1.5f, GlobalRect.Bottom - TextureSize.Height));
                }

                RenderBar(context, GlobalRect.Width - BarSize.Width);
                RenderDigits(context, FocusPart.Pn, new(GlobalRect.Right - TextureSize.Width * 1.5f, GlobalRect.Top));
                RenderDigits(context, FocusPart.Pd, new(GlobalRect.Right - TextureSize.Width * 1.5f, GlobalRect.Bottom - TextureSize.Height));
                break;
        }
    }

    private void RenderDigits(IDrawingContext context, FocusPart focusPart, PointF position)
    {
        var digits = FocusedPartDigits(focusPart);
        var length = FocusedPartLength(focusPart);
        var hasCursor = IsFocused && focusPart == _focusedPart;
        var digitCount = hasCursor && IsFocused && length < 3 ? length + 1 : length;
        var xOrigin = position.X - digitCount * TextureSize.Width * 0.5f;

        for (var i = 0; i < length; i++)
        {
            RenderDigit(digits[i], i);
        }

       if (hasCursor)
        {
            // カーソル
            RenderDigit(10, Math.Min(length, 2));
        }

        void RenderDigit(int digit, int index)
        {
            var destRect = new RectF(
                xOrigin + TextureSize.Width * index,
                position.Y,
                xOrigin + TextureSize.Width * (index + 1),
                position.Y + TextureSize.Height);
            context.DrawImage(in destRect, in Textures[digit]);
        }
    }

    private void RenderBar(IDrawingContext context, float x)
    {
        context.DrawImage(new RectF(
            GlobalRect.Left + x,
            GlobalRect.Top + _halfSize.Height - BarSize.Height * 0.5f,
            GlobalRect.Left + x + BarSize.Width,
            GlobalRect.Top + _halfSize.Height + BarSize.Height * 0.5f),
            in BarTexture);
    }

    private void RenderHat(IDrawingContext context)
    {
        context.DrawImage(new RectF(
            GlobalRect.Left + _halfSize.Width - HatSize.Width * 0.5f,
            GlobalRect.Top,
            GlobalRect.Left + _halfSize.Width + HatSize.Width * 0.5f,
            GlobalRect.Top + HatSize.Height),
            in HatTexture);
    }

    internal override bool TryFindParameter(PointF point, out int parameterId)
    {
        parameterId = point.X < _halfSize.Width
            ? point.Y < _halfSize.Height ? n.Id : d.Id
            : point.Y < _halfSize.Height ? pn.Id : pd.Id;
        return true;
    }

    private enum FocusPart
    {
        N,
        D,
        Pn,
        Pd,
    }
}
